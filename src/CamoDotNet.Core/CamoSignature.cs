// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography;
using System.Text;
using CamoDotNet.Core.Extensions;

namespace CamoDotNet.Core
{
    public class CamoSignature
    {
        private readonly HMAC _hmac;

        public CamoSignature(HMAC hmac)
        {
            _hmac = hmac;
            _hmac.Initialize();
        }

        public bool IsValidSignature(string url, string signature)
        {
            return signature == GenerateSignature(url);
        }

        public string GenerateSignature(string stringToSign)
        {
            return Encoding.ASCII.GetString(
                    _hmac.ComputeHash(Encoding.ASCII.GetBytes(stringToSign)))
                .ToHex();
        }
    }
}