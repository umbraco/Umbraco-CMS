using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Strings;

namespace Umbraco.Core
{

    ///<summary>
    /// String extension methods
    ///</summary>
    public static class StringExtensions
    {

        // FORMAT STRINGS

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="alias">The text to filter.</param>
        /// <returns>The safe alias.</returns>
        public static string ToSafeAlias(this string alias)
        {
            return Current.ShortStringHelper.CleanStringForSafeAlias(alias);
        }

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="alias">The text to filter.</param>
        /// <param name="camel">A value indicating that we want to camel-case the alias.</param>
        /// <returns>The safe alias.</returns>
        public static string ToSafeAlias(this string alias, bool camel)
        {
            var a = Current.ShortStringHelper.CleanStringForSafeAlias(alias);
            if (string.IsNullOrWhiteSpace(a) || camel == false) return a;
            return char.ToLowerInvariant(a[0]) + a.Substring(1);
        }

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="alias">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe alias.</returns>
        public static string ToSafeAlias(this string alias, string culture)
        {
            return Current.ShortStringHelper.CleanStringForSafeAlias(alias, culture);
        }

        // the new methods to get a url segment

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe url segment.</returns>
        public static string ToUrlSegment(this string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(text));

            return Current.ShortStringHelper.CleanStringForUrlSegment(text);
        }

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe url segment.</returns>
        public static string ToUrlSegment(this string text, string culture)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(text));

            return Current.ShortStringHelper.CleanStringForUrlSegment(text, culture);
        }

        // the new methods to clean a string (to alias, url segment...)

        /// <summary>
        /// Cleans a string.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default,
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the ICurrent.ShortStringHelper default culture.</remarks>
        public static string ToCleanString(this string text, CleanStringType stringType)
        {
            return Current.ShortStringHelper.CleanString(text, stringType);
        }

        /// <summary>
        /// Cleans a string, using a specified separator.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default,
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the ICurrent.ShortStringHelper default culture.</remarks>
        public static string ToCleanString(this string text, CleanStringType stringType, char separator)
        {
            return Current.ShortStringHelper.CleanString(text, stringType, separator);
        }

        /// <summary>
        /// Cleans a string in the context of a specified culture.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default,
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The clean string.</returns>
        public static string ToCleanString(this string text, CleanStringType stringType, string culture)
        {
            return Current.ShortStringHelper.CleanString(text, stringType, culture);
        }

        /// <summary>
        /// Cleans a string in the context of a specified culture, using a specified separator.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default,
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The clean string.</returns>
        public static string ToCleanString(this string text, CleanStringType stringType, char separator, string culture)
        {
            return Current.ShortStringHelper.CleanString(text, stringType, separator, culture);
        }

        // note: LegacyCurrent.ShortStringHelper will produce 100% backward-compatible output for SplitPascalCasing.
        // other helpers may not. DefaultCurrent.ShortStringHelper produces better, but non-compatible, results.

        /// <summary>
        /// Splits a Pascal cased string into a phrase separated by spaces.
        /// </summary>
        /// <param name="phrase">The text to split.</param>
        /// <returns>The split text.</returns>
        public static string SplitPascalCasing(this string phrase)
        {
            return Current.ShortStringHelper.SplitPascalCasing(phrase, ' ');
        }

        //NOTE: Not sure what this actually does but is used a few places, need to figure it out and then move to StringExtensions and obsolete.
        // it basically is yet another version of SplitPascalCasing
        // plugging string extensions here to be 99% compatible
        // the only diff. is with numbers, Number6Is was "Number6 Is", and the new string helper does it too,
        // but the legacy one does "Number6Is"... assuming it is not a big deal.
        internal static string SpaceCamelCasing(this string phrase)
        {
            return phrase.Length < 2 ? phrase : phrase.SplitPascalCasing().ToFirstUpperInvariant();
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe filename.</returns>
        public static string ToSafeFileName(this string text)
        {
            return Current.ShortStringHelper.CleanStringForSafeFileName(text);
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe filename.</returns>
        public static string ToSafeFileName(this string text, string culture)
        {
            return Current.ShortStringHelper.CleanStringForSafeFileName(text, culture);
        }




    }
}
