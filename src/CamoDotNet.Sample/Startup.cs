// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Net;
using System.Net.Http;
using CamoDotNet.Core;
using CamoDotNet.Sample;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup("CamoDotNet", typeof(Startup))]

namespace CamoDotNet.Sample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ServicePointManager.DefaultConnectionLimit = 50;

            var camoServerSettings = CamoServerSettings.GetDefault("TEST1234");
            var camoUrlHelper = new CamoUrlHelper(
                new CamoSignature(camoServerSettings.SharedKey), "/camo");

            app.UseCamoServer(
                "/camo",
                camoServerSettings,
                new HttpClient { Timeout = TimeSpan.FromSeconds(10) });

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value == "/")
                {
                    await context.Response.WriteAsync(@"
                        <html>
                          <head><title>CamoDotNet example</title></head>
                          <body>
                            <img src=""" + camoUrlHelper.GenerateUrl("http://31.media.tumblr.com/265d44b9503782d921a6695eddb4d4ae/tumblr_inline_ntk5zmSKP21raprkq_500.gif") + @""" />
                          </body>
                        </html>");
                }
            });
        }
    }
}