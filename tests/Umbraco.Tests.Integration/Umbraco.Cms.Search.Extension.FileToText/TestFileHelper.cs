// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Extension.FileToText.Tests;

internal static class TestFileHelper
{
    public static string LoremIpsumFilePath(string extension)
        => Path.Combine(TestContext.CurrentContext.TestDirectory, "Umbraco.Cms.Search.Extension.FileToText", "TestFiles", $"Lorem ipsum{extension}");

    public const string LoremIpsumStart = "Lorem ipsum dolor? Duis eros leo, malesuada id porta vel, eleifend eget enim. Suspendisse potenti.";
}
