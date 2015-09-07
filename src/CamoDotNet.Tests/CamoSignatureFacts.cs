// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System.Security.Cryptography;
using System.Text;
using CamoDotNet.Core;
using Xunit;

namespace CamoDotNet.Tests
{
    public class CamoSignatureFacts
    {
        [Theory]
        [InlineData("https://raw.githubusercontent.com/NuGet/Home/dev/resources/nuget.png", "10453F3F3F3F3F6D413F3F3F3F75493F3F3F3F73126E3F472F3F3F3F3F023F5F")]
        public void MatchesValidSignature(string url, string signature)
        {
            var target = new CamoSignature(new HMACSHA256(Encoding.ASCII.GetBytes("TEST1234")));
            var result = target.IsValidSignature(url, signature);

            Assert.True(result);
        }
    }
}
