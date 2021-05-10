// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using CamoDotNet.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace CamoDotNet.Tests
{
    public abstract class CamoServerFactsBase
    {
        private const string SharedKeyForTests = "TEST1234";

        protected virtual TestServer CreateServer()
        {
            return new TestServer(new WebHostBuilder()
                .UseStartup<TestStartup>());
        }

        protected string GenerateSignedUrl(string url)
        {
            var helper = new CamoUrlHelper(new CamoSignature(
                CamoServerSettings.GetDefault(SharedKeyForTests).SharedKey), "");

            return helper.GenerateUrl(url);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestStartup
        {
            // ReSharper disable once UnusedMember.Local
            public void Configure(IApplicationBuilder app)
            {
                app.UseCamoServer(
                    CamoServerSettings.GetDefault(SharedKeyForTests),
                    new HttpClient { Timeout = TimeSpan.FromSeconds(10) });
            }
        }
    }
}