// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CamoDotNet.Core.Extensions;
using Xunit;

namespace CamoDotNet.Tests;

public class StringExtensionsFacts
{
    [Theory]
    [InlineData("test", "74657374")]
    [InlineData("test1234", "7465737431323334")]
    [InlineData("TEST", "54455354")]
    [InlineData("TEST1234", "5445535431323334")]
    [InlineData("This is a test", "5468697320697320612074657374")]
    [InlineData("https://raw.githubusercontent.com/NuGet/Home/dev/resources/nuget.png", "68747470733A2F2F7261772E67697468756275736572636F6E74656E742E636F6D2F4E754765742F486F6D652F6465762F7265736F75726365732F6E756765742E706E67")]
    public void ToHexReturnsProperHex(string input, string expected)
    {
        var result = input.ToHex();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("74657374", "test")]
    [InlineData("7465737431323334", "test1234")]
    [InlineData("54455354", "TEST")]
    [InlineData("5445535431323334", "TEST1234")]
    [InlineData("5468697320697320612074657374", "This is a test")]
    [InlineData("68747470733A2F2F7261772E67697468756275736572636F6E74656E742E636F6D2F4E754765742F486F6D652F6465762F7265736F75726365732F6E756765742E706E67", "https://raw.githubusercontent.com/NuGet/Home/dev/resources/nuget.png")]
    public void FromHexReturnsProperString(string input, string expected)
    {
        var result = input.FromHex();
        Assert.Equal(expected, result);
    }
}