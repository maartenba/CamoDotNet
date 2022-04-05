// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CamoDotNet;
using CamoDotNet.Core;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var camoServerSettings = CamoServerSettings.GetDefault("TEST1234");
var camoUrlHelper = new CamoUrlHelper(
    new CamoSignature(camoServerSettings.SharedKey), "/camo");

app.UseCamoServer(
    "/camo",
    camoServerSettings,
    new HttpClient { Timeout = TimeSpan.FromSeconds(10) });

app.MapGet("/", () => Results.Content(@"<html>
      <head><title>CamoDotNet example</title></head>
      <body>
        <img src=""" + camoUrlHelper.GenerateUrl("http://31.media.tumblr.com/265d44b9503782d921a6695eddb4d4ae/tumblr_inline_ntk5zmSKP21raprkq_500.gif") + @""" />
      </body>
    </html>", "text/html"));

app.Run();