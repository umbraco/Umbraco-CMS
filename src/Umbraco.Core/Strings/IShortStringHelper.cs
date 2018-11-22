using System.Collections.Generic;
using System.Globalization;

namespace Umbraco.Core.Strings
{
    /// <summary>
    /// Provides string functions for short strings such as aliases or url segments.
    /// </summary>
    /// <remarks>Not necessarily optimized to work on large bodies of text.</remarks>
    public interface IShortStringHelper
    {
        /// <summary>
        /// Freezes the helper so it can prevents its configuration from being modified.
        /// </summary>
        /// <remarks>Will be called by <c>ShortStringHelperResolver</c> when resolution freezes.</remarks>
        void Freeze();

        /// <summary>
        /// Gets the JavaScript code defining client-side short string services.
        /// </summary>
        string GetShortStringServicesJavaScript(string controllerPath);

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe alias.</returns>
        /// <remarks>
        /// <para>The string will be cleaned in the context of the IShortStringHelper default culture.</para>
        /// <para>A safe alias is [a-z][a-zA-Z0-9_]* although legacy will also accept '-', and '_' at the beginning.</para>
        /// </remarks>
        string CleanStringForSafeAlias(string text);

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe alias.</returns>
        string CleanStringForSafeAlias(string text, CultureInfo culture);

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe url segment.</returns>
        /// <remarks>The string will be cleaned in the context of the IShortStringHelper default culture.</remarks>
        string CleanStringForUrlSegment(string text);

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe url segment.</returns>
        string CleanStringForUrlSegment(string text, CultureInfo culture);

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe filename.</returns>
        /// <remarks>Legacy says this was used to "overcome an issue when Umbraco is used in IE in an intranet environment" but that issue is not documented.</remarks>
        string CleanStringForSafeFileName(string text);

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe filename.</returns>
        /// <remarks>Legacy says this was used to "overcome an issue when Umbraco is used in IE in an intranet environment" but that issue is not documented.</remarks>
        string CleanStringForSafeFileName(string text, CultureInfo culture);

        /// <summary>
        /// Splits a pascal-cased string by inserting a separator in between each term.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The splitted string.</returns>
        /// <remarks>Supports Utf8 and Ascii strings, not Unicode strings.</remarks>
        string SplitPascalCasing(string text, char separator);

        /// <summary>
        /// Returns a new string in which all occurences of specified strings are replaced by other specified strings.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="replacements">The replacements definition.</param>
        /// <returns>The filtered string.</returns>
        string ReplaceMany(string text, IDictionary<string, string> replacements);

        /// <summary>
        /// Returns a new string in which all occurences of specified characters are replaced by a specified character.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="chars">The characters to replace.</param>
        /// <param name="replacement">The replacement character.</param>
        /// <returns>The filtered string.</returns>
        string ReplaceMany(string text, char[] chars, char replacement);

        /// <summary>
        /// Cleans a string.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the IShortStringHelper default culture.</remarks>
        string CleanString(string text, CleanStringType stringType);

        /// <summary>
        /// Cleans a string, using a specified separator.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the IShortStringHelper default culture.</remarks>
        string CleanString(string text, CleanStringType stringType, char separator);

        /// <summary>
        /// Cleans a string in the context of a specified culture.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The clean string.</returns>
        string CleanString(string text, CleanStringType stringType, CultureInfo culture);

        /// <summary>
        /// Cleans a string in the context of a specified culture, using a specified separator.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The clean string.</returns>
        string CleanString(string text, CleanStringType stringType, char separator, CultureInfo culture);
    }
}
