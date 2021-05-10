# CamoDotNet

CamoDotNet is a .NET port of [camo](https://github.com/atmos/camo). It is all about making insecure assets look secure. This is an SSL image proxy to prevent mixed content warnings on secure pages.

[Check the GitHub blog](https://github.com/blog/743-sidejack-prevention-phase-3-ssl-proxied-assets) for background on why camo exists.

Using a shared key, proxy URLs are encrypted with [hmac](http://en.wikipedia.org/wiki/HMAC) so we can bust caches/ban/rate limit if needed.

CamoDotNet currently runs on:

* CamoDotNet 1.x - OWIN 3.0
* CamoDotNet 2.x - .NET Core
* CamoDotNet 3.x - .NET Standard 2.0
* CamoDotNet 4.x - .NET Core 3.1

## Features

* Max size for proxied images
* Restricts proxied images content-types to a whitelist
* Forward images regardless of HTTP status code

## URL Formats

CamoDotNet supports two distinct URL formats:

    http://example.org/<digest>?url=<image-url>
    http://example.org/<digest>/<image-url>

The `<digest>` is a 40 character hex encoded HMAC digest generated with a shared secret key and the unescaped `<image-url>` value.

The `<image-url>` is the absolute URL locating an image. In the first format, the `<image-url>` should be
URL escaped aggressively to ensure the original value isn't mangled in transit.

In the second format, each byte of the `<image-url>` should be hex encoded such that the resulting value includes only characters `[0-9a-f]`.

## Usage

### Server

The CamoDotNet server is implemented as an OWIN middleware and can be added to any OWIN application, either as a middleware (using `IAppBuilder.Use`) or as the main server (`using IAppBuilder.Run`). The following example bootstraps a CamoDotNetServer under the `/camo` path.

    public class Startup
    {
        public void Configuration(IAppBuilder app) // or IApplicationBuilder in .NET Core
        {
            var camoServerSettings = CamoServerSettings.GetDefault("shared_key_goes_here");
            var camoUrlHelper = new CamoUrlHelper(
                new CamoSignature(camoServerSettings.SharedKey), "/camo");

            app.UseCamoServer(
                "/camo",
                camoServerSettings,
                new HttpClient { Timeout = TimeSpan.FromSeconds(10) });
        }
    }

The `CamoDotNet.Sample` project contains a minimal sample of embedding CamoDotNet in an application.

### Client

All the client has to to is render an `<img />` tag that references a proxied image. URLs can be generated manually, using the URL format described above. Another option is by using the `CamoDotNet.Core.CamoUrlHelper` class:

    var helper = new CamoUrlHelper(new CamoSignature(
	    CamoServerSettings.GetDefault("shared_key_goes_here").SharedKey), "https://camo-url/");
	
    return helper.GenerateUrl(url);

The `CamoDotNet.Sample` project contains a minimal sample  that renders an image proxied through CamoDotNet.

## Configuration

CamoDotNet comes with several configuration options which can be specified as a parameter to the CamoDotNet server.

* `SharedKey`: The shared key used to generate the HMAC digest.
* `UserAgent`: The string for Camo to include in the `Via` and `User-Agent` headers it sends in requests to origin servers. (default: `CamoDotNet Asset Proxy/1.0`)
* `ContentLengthLimit`: The maximum `Content-Length` Camo will proxy. (default: 5242880)


