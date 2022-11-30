// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.ModelsBuilder.Embedded;

public static class StringExtensions
{
    public static string ClearLf(this string s) => s.Replace("\r", string.Empty);
}
