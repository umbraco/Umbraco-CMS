namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     Provides string functions for short strings such as aliases or URL segments.
/// </summary>
/// <remarks>Not necessarily optimized to work on large bodies of text.</remarks>
public interface IShortStringHelper
{
    /// <summary>
    ///     Cleans a string to produce a string that can safely be used in an alias.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The safe alias.</returns>
    /// <remarks>
    ///     <para>The string will be cleaned in the context of the IShortStringHelper default culture.</para>
    ///     <para>A safe alias is [a-z][a-zA-Z0-9_]* although legacy will also accept '-', and '_' at the beginning.</para>
    /// </remarks>
    string CleanStringForSafeAlias(string text);

    /// <summary>
    ///     Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an alias.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The safe alias.</returns>
    string CleanStringForSafeAlias(string text, string culture);

    /// <summary>
    ///     Cleans a string to produce a string that can safely be used in an URL segment.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The safe URL segment.</returns>
    /// <remarks>The string will be cleaned in the context of the IShortStringHelper default culture.</remarks>
    string CleanStringForUrlSegment(string text);

    /// <summary>
    ///     Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an URL
    ///     segment.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The safe URL segment.</returns>
    string CleanStringForUrlSegment(string text, string? culture);

    /// <summary>
    ///     Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a
    ///     filename,
    ///     both internally (on disk) and externally (as a URL).
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The safe filename.</returns>
    /// <remarks>
    ///     Legacy says this was used to "overcome an issue when Umbraco is used in IE in an intranet environment" but
    ///     that issue is not documented.
    /// </remarks>
    string CleanStringForSafeFileName(string text);

    /// <summary>
    ///     Cleans a string, in the context of a specified culture, to produce a string that can safely be used as a filename,
    ///     both internally (on disk) and externally (as a URL).
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The safe filename.</returns>
    /// <remarks>
    ///     Legacy says this was used to "overcome an issue when Umbraco is used in IE in an intranet environment" but
    ///     that issue is not documented.
    /// </remarks>
    string CleanStringForSafeFileName(string text, string culture);

    /// <summary>
    ///     Splits a pascal-cased string by inserting a separator in between each term.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <param name="separator">The separator.</param>
    /// <returns>The split string.</returns>
    /// <remarks>Supports Utf8 and Ascii strings, not Unicode strings.</remarks>
    string SplitPascalCasing(string text, char separator);

    /// <summary>
    ///     Cleans a string.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="stringType">
    ///     A flag indicating the target casing and encoding of the string. By default,
    ///     strings are cleaned up to camelCase and Ascii.
    /// </param>
    /// <returns>The clean string.</returns>
    /// <remarks>The string is cleaned in the context of the IShortStringHelper default culture.</remarks>
    string CleanString(string text, CleanStringType stringType);

    /// <summary>
    ///     Cleans a string, using a specified separator.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="stringType">
    ///     A flag indicating the target casing and encoding of the string. By default,
    ///     strings are cleaned up to camelCase and Ascii.
    /// </param>
    /// <param name="separator">The separator.</param>
    /// <returns>The clean string.</returns>
    /// <remarks>The string is cleaned in the context of the IShortStringHelper default culture.</remarks>
    string CleanString(string text, CleanStringType stringType, char separator);

    /// <summary>
    ///     Cleans a string in the context of a specified culture.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="stringType">
    ///     A flag indicating the target casing and encoding of the string. By default,
    ///     strings are cleaned up to camelCase and Ascii.
    /// </param>
    /// <param name="culture">The culture.</param>
    /// <returns>The clean string.</returns>
    string CleanString(string text, CleanStringType stringType, string culture);

    /// <summary>
    ///     Cleans a string in the context of a specified culture, using a specified separator.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="stringType">
    ///     A flag indicating the target casing and encoding of the string. By default,
    ///     strings are cleaned up to camelCase and Ascii.
    /// </param>
    /// <param name="separator">The separator.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The clean string.</returns>
    string CleanString(string text, CleanStringType stringType, char separator, string culture);
}
