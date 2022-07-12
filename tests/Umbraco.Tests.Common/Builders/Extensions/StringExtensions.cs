// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Common.Builders.Extensions;

public static class StringExtensions
{
    public static string ToCamelCase(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return string.Empty;
        }

        if (s.Length == 1)
        {
            return s.ToLowerInvariant();
        }

        return char.ToLowerInvariant(s[0]) + s.Substring(1);
    }
}
