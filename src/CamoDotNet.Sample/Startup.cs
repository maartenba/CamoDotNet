// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using CamoDotNet.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CamoDotNet.Sample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
