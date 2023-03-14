// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

internal class MockShortStringHelper : IShortStringHelper
{
    public bool IsFrozen { get; private set; }

    public string CleanStringForSafeAlias(string text) => "SAFE-ALIAS::" + text;

    public string CleanStringForSafeAlias(string text, string culture) => "SAFE-ALIAS-CULTURE::" + text;

    public string CleanStringForUrlSegment(string text) => "URL-SEGMENT::" + text;

    public string CleanStringForUrlSegment(string text, string culture) => "URL-SEGMENT-CULTURE::" + text;

    public string CleanStringForSafeFileName(string text) => "SAFE-FILE-NAME::" + text;

    public string CleanStringForSafeFileName(string text, string culture) => "SAFE-FILE-NAME-CULTURE::" + text;

    public string SplitPascalCasing(string text, char separator) => "SPLIT-PASCAL-CASING::" + text;

    public string CleanString(string text, CleanStringType stringType) => "CLEAN-STRING-A::" + text;

    public string CleanString(string text, CleanStringType stringType, char separator) => "CLEAN-STRING-B::" + text;

    public string CleanString(string text, CleanStringType stringType, string culture) => "CLEAN-STRING-C::" + text;

    public string CleanString(string text, CleanStringType stringType, char separator, string culture) =>
        "CLEAN-STRING-D::" + text;

    public void Freeze() => IsFrozen = true;
}
