// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CamoDotNet.Tests
{
    public class CamoServerProxyFacts
        : CamoServerFactsBase
    {
        [Fact]
        public async Task ProxiesValidUrl()
        {
            using (var server = CreateServer())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(GenerateSignedUrl("https://raw.githubusercontent.com/NuGet/Home/dev/resources/nuget.png"));

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.IsType<StreamContent>(response.Content);
            }
        }

        [Fact]
        public async Task DoesNotProxyInvalidContentType()
        {
            using (var server = CreateServer())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(GenerateSignedUrl("https://www.github.com"));

                Assert.Contains("Non-Image content-type returned", await response.Content.ReadAsStringAsync());
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task DoesNotProxyInvalidUrl()
        {
            using (var server = CreateServer())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(GenerateSignedUrl("https://www.github.com.thisdoesnotexist"));
                
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }
    }
}