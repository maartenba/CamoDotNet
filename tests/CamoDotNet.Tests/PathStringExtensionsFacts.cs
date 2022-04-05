// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using CamoDotNet.Extensions;
using Xunit;

namespace CamoDotNet.Tests;

public class PathStringExtensionsFacts
{
    [Theory]
    [InlineData("/foo/bar/baz", "/foo", "/bar/baz")]
    [InlineData("/foo/bar/baz", "/foo/bar", "/baz")]
    public void RemovesPathPrefixFromPathString(string current, string prefix, string expected)
    {
        var result = new PathString(current).RemovePrefix(new PathString(prefix));

        Assert.Equal(expected, result.Value);
    }
}