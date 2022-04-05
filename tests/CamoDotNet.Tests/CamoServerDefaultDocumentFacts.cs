// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CamoDotNet.Tests;

public class CamoServerSignatureFacts
    : CamoServerFactsBase
{
    [Fact]
    public async Task Returns404NotFoundForChecksumMismatchWithPathFormat()
    {
        using var server = CreateServer();
        var response = await server.CreateClient().GetAsync("/74657374/68747470733a2f2f7261772e67697468756275736572636f6e74656e742e636f6d2f4e754765742f486f6d652f6465762f7265736f75726365732f6e756765742e706e67");

        Assert.Contains("Checksum mismatch", await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Returns404NotFoundForChecksumMismatchWithQueryFormat()
    {
        using var server = CreateServer();
        var response = await server.CreateClient().GetAsync("/74657374?url=http%3A%2F%2Fwww.nuget.org%2Ftest");

        Assert.Contains("Checksum mismatch", await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
public class CamoServerDefaultDocumentFacts
    : CamoServerFactsBase
{
    [Fact]
    public async Task Returns405MethodNotAllowedForPostToRootUrl()
    {
        using var server = CreateServer();
        var response = await server.CreateClient().PostAsync("/", new StringContent("data"));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Returns200OkForRootUrl()
    {
        using var server = CreateServer();
        var response = await server.CreateClient().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Returns200OkForFavIconUrl()
    {
        using var server = CreateServer();
        var response = await server.CreateClient().GetAsync("/favicon.ico");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}