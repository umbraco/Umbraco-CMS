using Umbraco.Core.Strings;

namespace Umbraco.Core
{
    ///<summary>
    /// String extension methods
    ///</summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Splits a Pascal cased string into a phrase separated by spaces.
        /// </summary>
        /// <param name="phrase">The text to split.</param>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <returns>The split text.</returns>
        public static string SplitPascalCasing(this string phrase, IShortStringHelper shortStringHelper)
        {
            return shortStringHelper.SplitPascalCasing(phrase, ' ');
        }

        //NOTE: Not sure what this actually does but is used a few places, need to figure it out and then move to StringExtensions and obsolete.
        // it basically is yet another version of SplitPascalCasing
        // plugging string extensions here to be 99% compatible
        // the only diff. is with numbers, Number6Is was "Number6 Is", and the new string helper does it too,
        // but the legacy one does "Number6Is"... assuming it is not a big deal.
        internal static string SpaceCamelCasing(this string phrase, IShortStringHelper shortStringHelper)
        {
            return phrase.Length < 2 ? phrase : phrase.SplitPascalCasing(shortStringHelper).ToFirstUpperInvariant();
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <returns>The safe filename.</returns>
        public static string ToSafeFileName(this string text, IShortStringHelper shortStringHelper)
        {
            return shortStringHelper.CleanStringForSafeFileName(text);
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe filename.</returns>
        public static string ToSafeFileName(this string text, IShortStringHelper shortStringHelper, string culture)
        {
            return shortStringHelper.CleanStringForSafeFileName(text, culture);
        }
    }
}
