// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CamoDotNet.Core;
using CamoDotNet.Core.Extensions;
using CamoDotNet.Extensions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace CamoDotNet;

[PublicAPI]
public class CamoServer
{
    private readonly PathString _pathPrefix;
    private readonly CamoServerSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly CamoSignature _signature;

    private readonly string[] _supportedMediaTypes =
    {
        "image/bmp",
        "image/cgm",
        "image/g3fax",
        "image/gif",
        "image/ief",
        "image/jp2",
        "image/jpeg",
        "image/jpg",
        "image/pict",
        "image/png",
        "image/prs.btif",
        "image/svg+xml",
        "image/tiff",
        "image/vnd.adobe.photoshop",
        "image/vnd.djvu",
        "image/vnd.dwg",
        "image/vnd.dxf",
        "image/vnd.fastbidsheet",
        "image/vnd.fpx",
        "image/vnd.fst",
        "image/vnd.fujixerox.edmics-mmr",
        "image/vnd.fujixerox.edmics-rlc",
        "image/vnd.microsoft.icon",
        "image/vnd.ms-modi",
        "image/vnd.net-fpx",
        "image/vnd.wap.wbmp",
        "image/vnd.xiff",
        "image/webp",
        "image/x-cmu-raster",
        "image/x-cmx",
        "image/x-icon",
        "image/x-macpaint",
        "image/x-pcx",
        "image/x-pict",
        "image/x-portable-anymap",
        "image/x-portable-bitmap",
        "image/x-portable-graymap",
        "image/x-portable-pixmap",
        "image/x-quicktime",
        "image/x-rgb",
        "image/x-xbitmap",
        "image/x-xpixmap",
        "image/x-xwindowdump"
    };

    private readonly Dictionary<string, string> _defaultHeaders = new()
    {
        {"X-Frame-Options", "deny"},
        {"X-XSS-Protection", "1; mode=block"},
        {"X-Content-Type-Options", "nosniff"},
        {"Content-Security-Policy", "default-src 'none'; img-src data:; style-src 'unsafe-inline'"}
        //{ "Strict-Transport-Security", "max-age=31536000; includeSubDomains" }
    };

    public CamoServer(CamoServerSettings settings, HttpClient httpClient)
        : this(new PathString(), settings, httpClient)
    {
    }

    public CamoServer(PathString pathPrefix, CamoServerSettings settings, HttpClient httpClient)
    {
        _pathPrefix = pathPrefix;
        _settings = settings;
        _httpClient = httpClient;
        _signature = new CamoSignature(_settings.SharedKey);
    }

    public async Task Invoke(HttpContext context)
    {
        // does our path match?
        if (!context.Request.Path.StartsWithSegments(_pathPrefix))
        {
            return;
        }

        // check request method
        if (context.Request.Method != "GET")
        {
            await WriteHead(context.Response, HttpStatusCode.MethodNotAllowed, _defaultHeaders);
            return;
        }

        // check incoming URL
        var requestPath = context.Request.Path.RemovePrefix(_pathPrefix);
        switch (requestPath.Value)
        {
            case "/":
            case "/favicon.ico":
                await WriteHead(context.Response, HttpStatusCode.OK, _defaultHeaders);
                return;
        }
            
        // parse parameters
        var parameters = requestPath.Value!.TrimStart('/').Split(new [] { '/' }, 2);

        string url;
        var signature = parameters[0];
        if (parameters.Length == 2)
        {
            url = parameters[1].FromHex();
        }
        else
        {
            url = context.Request.Query["url"];
        }

        // validate signature
        if (!_signature.IsValidSignature(url, signature))
        {
            await WriteInvalidSignature(context.Response, url, signature);
            return;
        }

        // is it a loop?
        if ((context.Request.Headers.ContainsKey("User-Agent") && context.Request.Headers["User-Agent"] == _settings.UserAgent)
            || (context.Request.Headers.ContainsKey("Via") && context.Request.Headers["Via"].Contains(_settings.UserAgent)))
        {
            await WriteHead(context.Response, HttpStatusCode.BadRequest, _defaultHeaders);
            return;
        }

        // proxy the request
        var upstreamRequest = new HttpRequestMessage(HttpMethod.Get, url);
        upstreamRequest.Headers.UserAgent.ParseAdd(_settings.UserAgent);
        upstreamRequest.Headers.Via.ParseAdd("1.1 camo (" + _settings.UserAgent + ")");
        await TransferHeaders(context.Request.Headers, upstreamRequest.Headers);
        HttpResponseMessage upstreamResponse;
        try
        {
            upstreamResponse = await _httpClient.SendAsync(upstreamRequest);
        }
        catch (HttpRequestException)
        {
            await WriteHead(context.Response, HttpStatusCode.BadRequest, _defaultHeaders);
            return;
        }

        await using var upstreamResponseStream = await upstreamResponse.Content.ReadAsStreamAsync();
        
        // validate response
        if (upstreamResponseStream.Length > _settings.ContentLengthLimit)
        {
            await WriteContentLengthExceeded(context.Response, _defaultHeaders);
            return;
        }

        var contentTypes = upstreamResponse.Content.Headers.ContentType != null
            ? upstreamResponse.Content.Headers.ContentType.MediaType?.Split(new [] { ";" }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()
            : Array.Empty<string>();
        if (contentTypes.Length == 0 || !_supportedMediaTypes.Any(
                mt => contentTypes.Any(ct => ct.Equals(mt, StringComparison.OrdinalIgnoreCase))))
        {
            await WriteContentTypeUnsupported(context.Response, upstreamResponse.Content.Headers.ContentType?.MediaType, _defaultHeaders);
            return;
        }

        // stream response
        var headers = new Dictionary<string, string>(_defaultHeaders)
        {
            { "Via", "1.1 camo (" + _settings.UserAgent + ")" }
        };
        if (upstreamResponse.Headers.ETag != null)
        {
            headers.Add("Etag", upstreamResponse.Headers.ETag.ToString());
        }
        if (upstreamResponse.Content.Headers.LastModified.HasValue)
        {
            headers.Add("last-modified", upstreamResponse.Content.Headers.LastModified.ToString()!);
        }
                
        context.Response.StatusCode = (int) upstreamResponse.StatusCode;
        await WriteHeaders(context.Response, headers);
        context.Response.ContentType = upstreamResponse.Content.Headers.ContentType!.ToString();

        await upstreamResponseStream.CopyToAsync(context.Response.Body);
    }

    private Task TransferHeaders(IHeaderDictionary sourceHeaders, HttpRequestHeaders destinationHeaders)
    {
        destinationHeaders.Add("Accept", sourceHeaders.ContainsKey("Accept")
            ? sourceHeaders["Accept"].ToString()
            : "image/*");

        destinationHeaders.Add("Accept-Encoding", sourceHeaders.ContainsKey("Accept-Encoding")
            ? sourceHeaders["Accept-Encoding"].ToString()
            : string.Empty);

        destinationHeaders.Add("X-Frame-Options", _defaultHeaders["X-Frame-Options"]);

        destinationHeaders.Add("X-XSS-Protection", _defaultHeaders["X-XSS-Protection"]);

        destinationHeaders.Add("X-Content-Type-Options", _defaultHeaders["X-Content-Type-Options"]);

        //destinationHeaders.Add("Content-Security-Policy", _defaultHeaders["Content-Security-Policy"]);

        return Task.CompletedTask;
    }

    private static Task WriteHeaders(HttpResponse response, Dictionary<string, string> headers)
    {
        foreach (var (key, value) in headers)
        {
            response.Headers.Append(key, value);
        }

        return Task.CompletedTask;
    }

    private static async Task WriteInvalidSignature(HttpResponse response, string url, string signature)
    {
        response.StatusCode = (int)HttpStatusCode.NotFound;
        await response.WriteAsync($"Checksum mismatch {url}:{signature}");
    }

    private static async Task WriteContentLengthExceeded(HttpResponse response, Dictionary<string, string> headers)
    {
        response.StatusCode = (int)HttpStatusCode.NotFound;
        await WriteHeaders(response, headers);
        await response.WriteAsync("Content-Length exceeded");
    }

    private static async Task WriteContentTypeUnsupported(HttpResponse response, string? contentTypeReturned, Dictionary<string, string> headers)
    {
        response.StatusCode = (int)HttpStatusCode.NotFound;
        await WriteHeaders(response, headers);
        await response.WriteAsync($"Non-image content-type returned '{contentTypeReturned ?? "unspecified"}'");
    }

    private static async Task WriteHead(HttpResponse response, HttpStatusCode statusCode, Dictionary<string, string> headers)
    {
        response.StatusCode = (int)statusCode;
        await WriteHeaders(response, headers);
    }
}