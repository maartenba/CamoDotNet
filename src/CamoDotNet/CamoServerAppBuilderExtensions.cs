// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System.Net.Http;
using Microsoft.Owin;
using Owin;

namespace CamoDotNet
{
    public static class CamoServerAppBuilderExtensions
    {
        public static IAppBuilder UseCamoServer(this IAppBuilder builder, CamoServerSettings settings, HttpClient httpClient)
        {
            return UseCamoServer(builder, new PathString(), settings, httpClient);
        }

        public static IAppBuilder UseCamoServer(this IAppBuilder builder, string pathMatch, CamoServerSettings settings, HttpClient httpClient)
        {
            return UseCamoServer(builder, new PathString(pathMatch), settings, httpClient);
        }

        public static IAppBuilder UseCamoServer(this IAppBuilder builder, PathString pathMatch, CamoServerSettings settings, HttpClient httpClient)
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
}