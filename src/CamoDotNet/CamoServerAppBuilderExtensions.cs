// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CamoDotNet;

[PublicAPI]
public static class CamoServerApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCamoServer(this IApplicationBuilder builder, CamoServerSettings settings, HttpClient httpClient)
    {
        return UseCamoServer(builder, new PathString(), settings, httpClient);
    }

    public static IApplicationBuilder UseCamoServer(this IApplicationBuilder builder, string pathMatch, CamoServerSettings settings, HttpClient httpClient)
    {
        return UseCamoServer(builder, new PathString(pathMatch), settings, httpClient);
    }

    public static IApplicationBuilder UseCamoServer(this IApplicationBuilder builder, PathString pathMatch, CamoServerSettings settings, HttpClient httpClient)
    {
        var server = new CamoServer(pathMatch, settings, httpClient);

        builder.Use(async (context, next) =>
        {
            await next();
            await server.Invoke(context);
        });

        return builder;
    }
}