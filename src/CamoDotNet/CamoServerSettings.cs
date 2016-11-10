// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography;
using System.Text;

namespace CamoDotNet
{
    public class CamoServerSettings
    {
        private const string DefaultUserAgent = "CamoDotNet Asset Proxy/2.0.0";

        public HMAC SharedKey { get; private set; }
        public string UserAgent { get; set; }
        public int ContentLengthLimit { get; private set; }

        public CamoServerSettings(HMACSHA256 sharedKey, string userAgent, int contentLengthLimit)
        {
            SharedKey = sharedKey;
            UserAgent = userAgent;
            ContentLengthLimit = contentLengthLimit;
        }

        public static CamoServerSettings GetDefault(string sharedKey)
        {
            return new CamoServerSettings(
                new HMACSHA256(Encoding.ASCII.GetBytes(sharedKey)),
                DefaultUserAgent,
                5242880);
        }
    }
}