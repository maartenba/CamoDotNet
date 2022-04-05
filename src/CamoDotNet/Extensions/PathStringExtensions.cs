// Copyright (c) Maarten Balliauw. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace CamoDotNet.Extensions;

public static class PathStringExtensions
{
    public static PathString RemovePrefix(this PathString current, PathString prefix) 
        => current.HasValue && prefix.HasValue
            ? new PathString(current.Value.Substring(prefix.Value.Length)) 
            : current;
}