// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

internal class MockShortStringHelper : IShortStringHelper
{
    /// <summary>
    /// Gets a value indicating whether this instance is frozen.
    /// </summary>
    public bool IsFrozen { get; private set; }

    /// <summary>
    /// Cleans the input string to produce a safe alias string.
    /// </summary>
    /// <param name="text">The input string to clean.</param>
    /// <returns>A safe alias string based on the input.</returns>
    public string CleanStringForSafeAlias(string text) => "SAFE-ALIAS::" + text;

    /// <summary>
    /// Cleans the specified string to create a safe alias.
    /// </summary>
    /// <param name="text">The string to clean.</param>
    /// <param name="culture">The culture to use for cleaning.</param>
    /// <returns>A cleaned string safe for use as an alias.</returns>
    public string CleanStringForSafeAlias(string text, string culture) => "SAFE-ALIAS-CULTURE::" + text;

    /// <summary>
    /// Cleans the specified string to make it suitable for use as a URL segment.
    /// </summary>
    /// <param name="text">The string to clean.</param>
    /// <returns>A cleaned string formatted for URL segments.</returns>
    public string CleanStringForUrlSegment(string text) => "URL-SEGMENT::" + text;

    /// <summary>
    /// Cleans the specified string to make it suitable for use as a URL segment.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="culture">The culture to use for cleaning.</param>
    /// <returns>A cleaned string suitable for URL segments.</returns>
    public string CleanStringForUrlSegment(string text, string culture) => "URL-SEGMENT-CULTURE::" + text;

    /// <summary>
    /// Cleans the input string to make it safe for use as a file name.
    /// </summary>
    /// <param name="text">The input string to clean.</param>
    /// <returns>A cleaned string safe for file names.</returns>
    public string CleanStringForSafeFileName(string text) => "SAFE-FILE-NAME::" + text;

    /// <summary>
    /// Cleans the specified string to make it safe for use as a file name.
    /// </summary>
    /// <param name="text">The input string to clean.</param>
    /// <param name="culture">The culture information used for cleaning.</param>
    /// <returns>A cleaned string safe for file names.</returns>
    public string CleanStringForSafeFileName(string text, string culture) => "SAFE-FILE-NAME-CULTURE::" + text;

    /// <summary>
    /// Splits a PascalCase string by inserting the specified separator.
    /// </summary>
    /// <param name="text">The PascalCase string to split.</param>
    /// <param name="separator">The character to insert between words.</param>
    /// <returns>The modified string with separators inserted.</returns>
    public string SplitPascalCasing(string text, char separator) => "SPLIT-PASCAL-CASING::" + text;

    /// <summary>
    /// Cleans the specified string based on the given clean string type.
    /// </summary>
    /// <param name="text">The string to clean.</param>
    /// <param name="stringType">The type of cleaning to apply.</param>
    /// <returns>The cleaned string.</returns>
    public string CleanString(string text, CleanStringType stringType) => "CLEAN-STRING-A::" + text;

    /// <summary>
    /// Cleans the specified string according to the given string type and separator.
    /// </summary>
    /// <param name="text">The string to clean.</param>
    /// <param name="stringType">The type of cleaning to apply.</param>
    /// <param name="separator">The character used as a separator in the cleaned string.</param>
    /// <returns>The cleaned string.</returns>
    public string CleanString(string text, CleanStringType stringType, char separator) => "CLEAN-STRING-B::" + text;

    /// <summary>
    /// Cleans the specified string based on the given string type and culture.
    /// </summary>
    /// <param name="text">The input string to clean.</param>
    /// <param name="stringType">The type of cleaning to perform on the string.</param>
    /// <param name="culture">The culture to use for cleaning rules.</param>
    /// <returns>The cleaned string.</returns>
    public string CleanString(string text, CleanStringType stringType, string culture) => "CLEAN-STRING-C::" + text;

    /// <summary>
    /// Mock implementation that returns a cleaned string for testing purposes, prefixing the input text with a fixed string.
    /// </summary>
    /// <param name="text">The string to clean.</param>
    /// <param name="stringType">The type of cleaning to apply (ignored in this mock).</param>
    /// <param name="separator">The character used as a separator in the cleaned string (ignored in this mock).</param>
    /// <param name="culture">The culture information used for cleaning (ignored in this mock).</param>
    /// <returns>The input <paramref name="text"/> prefixed with <c>"CLEAN-STRING-A::"</c>.</returns>
    public string CleanString(string text, CleanStringType stringType, char separator, string culture) =>
        "CLEAN-STRING-D::" + text;

    /// <summary>
    /// Freezes the current instance, preventing further modifications.
    /// </summary>
    public void Freeze() => IsFrozen = true;
}
