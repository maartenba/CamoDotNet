// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CamoDotNet.Core.Extensions
{
    public static class StringExtensions
    {
        public static string FromHex(this string from)
        {
            var result = "";
            while (from.Length > 0)
            {
                result += Convert.ToChar(Convert.ToUInt32(from.Substring(0, 2), 16)).ToString();
                from = from.Substring(2, from.Length - 2);
            }
            return result;
        }

        public static string ToHex(this string from)
        {
            var result = "";
            foreach (char c in from)
            {
                int tmp = c;
                result += string.Format("{0:X2}", Convert.ToUInt32(tmp.ToString()));
            }
            return result;
        }
    }
}