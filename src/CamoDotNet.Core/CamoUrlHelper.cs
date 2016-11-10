// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CamoDotNet.Core.Extensions;

namespace CamoDotNet.Core
{
    public class CamoUrlHelper
    {
        private readonly CamoSignature _signature;
        private readonly string _serverUrl;

        public CamoUrlHelper(CamoSignature signature, string serverUrl)
        {
            _signature = signature;
            _serverUrl = serverUrl;
        }

        public string GenerateUrl(string originalUrl)
        {
            return string.Format("{0}/{1}/{2}", _serverUrl.TrimEnd('/'), _signature.GenerateSignature(originalUrl), originalUrl.ToHex());
        }
    }
}
