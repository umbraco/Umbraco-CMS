using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Strings
{
    /// <summary>
    /// Legacy implementation of string functions for short strings such as aliases or url segments.
    /// </summary>
    /// <remarks>
    /// <para>Not necessarily optimized to work on large bodies of text.</para>
    /// <para>Can expose surprising or bogus behavior.</para>
    /// <para>Uses invariant culture everywhere.</para>
    /// </remarks>
    internal class LegacyShortStringHelper : IShortStringHelper
    {
        #region Ctor and vars

        /// <summary>
        /// Freezes the helper so it can prevents its configuration from being modified.
        /// </summary>
        /// <remarks>Will be called by <c>ShortStringHelperResolver</c> when resolution freezes.</remarks>
        public void Freeze()
        {
            // we have nothing to protect.
        }

        const string UmbracoValidAliasCharacters = "_-abcdefghijklmnopqrstuvwxyz1234567890";
        const string UmbracoInvalidFirstCharacters = "0123456789";

        #endregion

        #region Short string services JavaScript

        const string SssjsValidCharacters = "_-abcdefghijklmnopqrstuvwxyz1234567890";
        const string SssjsInvalidFirstCharacters = "0123456789";

        private const string SssjsFormat = @"
var UMBRACO_FORCE_SAFE_ALIAS = {0};
var UMBRACO_FORCE_SAFE_ALIAS_VALIDCHARS = '{1}';
var UMBRACO_FORCE_SAFE_ALIAS_INVALID_FIRST_CHARS = '{2}';

function safeAlias(alias) {{
    if (UMBRACO_FORCE_SAFE_ALIAS) {{
        var safeAlias = '';
        var aliasLength = alias.length;
        for (var i = 0; i < aliasLength; i++) {{
            currentChar = alias.substring(i, i + 1);
            if (UMBRACO_FORCE_SAFE_ALIAS_VALIDCHARS.indexOf(currentChar.toLowerCase()) > -1) {{
                // check for camel (if previous character is a space, we'll upper case the current one
                if (safeAlias == '' && UMBRACO_FORCE_SAFE_ALIAS_INVALID_FIRST_CHARS.indexOf(currentChar.toLowerCase()) > 0) {{ 
                    currentChar = '';
                }} else {{
                    // first char should always be lowercase (camel style)
                    if (safeAlias.length == 0)
                        currentChar = currentChar.toLowerCase();

                    if (i < aliasLength - 1 && safeAlias != '' && alias.substring(i - 1, i) == ' ')
                        currentChar = currentChar.toUpperCase();

                    safeAlias += currentChar;
                }}
            }}
        }}

        alias = safeAlias;
    }}
    return alias;
}}

function getSafeAlias(input, value, immediate, callback) {{
    callback(safeAlias(value));
}}

function validateSafeAlias(input, value, immediate, callback) {{
    callback(value == safeAlias(value));
}}

// legacy backward compatibility requires that one
function isValidAlias(alias) {{
    return alias == safeAlias(alias);
}}
";

        /// <summary>
        /// Gets the JavaScript code defining client-side short string services.
        /// </summary>
        public string GetShortStringServicesJavaScript(string controllerPath)
        {
            return string.Format(SssjsFormat,
                UmbracoConfig.For.UmbracoSettings().Content.ForceSafeAliases ? "true" : "false", SssjsValidCharacters, SssjsInvalidFirstCharacters);
        }

        #endregion

        #region IShortStringHelper CleanFor...

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe alias.</returns>
        /// <remarks>The string will be cleaned in the context of invariant culture.</remarks>
        public string CleanStringForSafeAlias(string text)
        {
            // ported from StringExtensions.ToSafeAlias()

            const string validAliasCharacters = UmbracoValidAliasCharacters;
            const string invalidFirstCharacters = UmbracoInvalidFirstCharacters;
            var safeString = new StringBuilder();
            int aliasLength = text.Length;
            for (var i = 0; i < aliasLength; i++)
            {
                var currentChar = text.Substring(i, 1);
                if (validAliasCharacters.Contains(currentChar.ToLowerInvariant()))
                {
                    // check for camel (if previous character is a space, we'll upper case the current one
                    if (safeString.Length == 0 && invalidFirstCharacters.Contains(currentChar.ToLowerInvariant()))
                    {
                        //currentChar = "";
                    }
                    else
                    {
                        if (i < aliasLength - 1 && i > 0 && text.Substring(i - 1, 1) == " ")
                            currentChar = currentChar.ToUpperInvariant();

                        safeString.Append(currentChar);
                    }
                }
            }

            return safeString.ToString();
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe alias.</returns>
        /// <remarks>Legacy does not support culture contexts.</remarks>
        public string CleanStringForSafeAlias(string text, CultureInfo culture)
        {
            return CleanStringForSafeAlias(text);
        }

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an url segment, in the context of the invariant culture.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe url segment.</returns>
        public string CleanStringForUrlSegment(string text)
        {
            return LegacyFormatUrl(text);
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe url segment.</returns>
        /// <remarks>Legacy does not support culture contexts.</remarks>
        public string CleanStringForUrlSegment(string text, CultureInfo culture)
        {
            return CleanStringForUrlSegment(text);
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe filename.</returns>
        /// <remarks>Legacy says this was used to "overcome an issue when Umbraco is used in IE in an intranet environment" but that issue is not documented.</remarks>
        public string CleanStringForSafeFileName(string text)
        {
            var filePath = text;

            // ported from Core.IO.IOHelper.SafeFileName()

            if (String.IsNullOrEmpty(filePath))
                return String.Empty;

            if (!String.IsNullOrWhiteSpace(filePath))
            {
                foreach (var character in Path.GetInvalidFileNameChars())
                {
                    filePath = filePath.Replace(character, '-');
                }
            }
            else
            {
                filePath = String.Empty;
            }

            //Break up the file in name and extension before applying the UrlReplaceCharacters
            var fileNamePart = filePath.Substring(0, filePath.LastIndexOf('.'));
            var ext = filePath.Substring(filePath.LastIndexOf('.'));

            //Because the file usually is downloadable as well we check characters against 'UmbracoSettings.UrlReplaceCharacters'
            foreach (var n in UmbracoConfig.For.UmbracoSettings().RequestHandler.CharCollection)
            {
                if (n.Char.IsNullOrWhiteSpace() == false)
                    fileNamePart = fileNamePart.Replace(n.Char, n.Replacement);
            }

            filePath = string.Concat(fileNamePart, ext);

            // Adapted from: http://stackoverflow.com/a/4827510/5018
            // Combined both Reserved Characters and Character Data 
            // from http://en.wikipedia.org/wiki/Percent-encoding
            var stringBuilder = new StringBuilder();

            const string reservedCharacters = "!*'();:@&=+$,/?%#[]-~{}\"<>\\^`| ";

            foreach (var character in filePath)
            {
                if (reservedCharacters.IndexOf(character) == -1)
                    stringBuilder.Append(character);
                else
                    stringBuilder.Append("-");
            }

            // Remove repeating dashes
            // From: http://stackoverflow.com/questions/5111967/regex-to-remove-a-specific-repeated-character
            var reducedString = Regex.Replace(stringBuilder.ToString(), "-+", "-");

            return reducedString;
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe filename.</returns>
        /// <remarks>Legacy does not support culture contexts.</remarks>
        public string CleanStringForSafeFileName(string text, CultureInfo culture)
        {
            return CleanStringForSafeFileName(text);
        }

        #endregion

        #region CleanString

        // legacy does not implement these

        public string CleanString(string text, CleanStringType stringType)
        {
            return text;
        }

        public string CleanString(string text, CleanStringType stringType, char separator)
        {
            return text;
        }

        public string CleanString(string text, CleanStringType stringType, CultureInfo culture)
        {
            return text;
        }

        public string CleanString(string text, CleanStringType stringType, char separator, CultureInfo culture)
        {
            return text;
        }
        
        #endregion

        #region SplitPascalCasing

        /// <summary>
        /// Splits a pascal-cased string by inserting a separator in between each term.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The splitted string.</returns>
        /// <remarks>Probably only supports Ascii strings.</remarks>
        public string SplitPascalCasing(string text, char separator)
        {
            // ported from StringExtensions.SplitPascalCasing()

            var replacement = "$1" + separator;
            var result = Regex.Replace(text, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", replacement);
            return result;
        }
        
        #endregion

        #region Legacy

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe alias.</returns>
        /// <remarks>The string will be cleaned in the context of invariant culture.</remarks>
        public string LegacyCleanStringForUmbracoAlias(string text)
        {
            // ported from StringExtensions.ToUmbracoAlias()
            // kept here for reference, not used anymore

            if (string.IsNullOrEmpty(text)) return string.Empty;

            //convert case first
            //var tmp = text.ConvertCase(caseType);
            // note: always Camel anyway
            var tmp = LegacyConvertStringCase(text, CleanStringType.CamelCase);

            //remove non-alphanumeric chars
            var result = Regex.Replace(tmp, @"[^a-zA-Z0-9\s\.-]+", "", RegexOptions.Compiled);

            // note: spaces are always removed anyway
            //if (removeSpaces)
            //    result = result.Replace(" ", "");

            return result;
        }

        /// <summary>
        /// Filters a string to convert case, and more.
        /// </summary>
        /// <param name="phrase">the text to filter.</param>
        /// <param name="cases">The string case type.</param>
        /// <returns>The filtered text.</returns>
        /// <remarks>
        /// <para>This is the legacy method, so we can't really change it, although it has issues (see unit tests).</para>
        /// <para>It does more than "converting the case", and also remove spaces, etc.</para>
        /// </remarks>
        public string LegacyConvertStringCase(string phrase, CleanStringType cases)
        {
            // ported from StringExtensions.ConvertCase

            cases &= CleanStringType.CaseMask;

            var splittedPhrase = Regex.Split(phrase, @"[^a-zA-Z0-9\']", RegexOptions.Compiled);

            if (cases == CleanStringType.Unchanged)
                return string.Join("", splittedPhrase);

            //var splittedPhrase = phrase.Split(' ', '-', '.');
            var sb = new StringBuilder();

            foreach (var splittedPhraseChars in splittedPhrase.Select(s => s.ToCharArray()))
            {
                if (splittedPhraseChars.Length > 0)
                {
                    splittedPhraseChars[0] = ((new String(splittedPhraseChars[0], 1)).ToUpperInvariant().ToCharArray())[0];
                }
                sb.Append(new String(splittedPhraseChars));
            }

            var result = sb.ToString();

            if (cases == CleanStringType.CamelCase)
            {
                if (result.Length > 1)
                {
                    var pattern = new Regex("^([A-Z]*)([A-Z].*)$", RegexOptions.Singleline | RegexOptions.Compiled);
                    var match = pattern.Match(result);
                    if (match.Success)
                    {
                        result = match.Groups[1].Value.ToLowerInvariant() + match.Groups[2].Value;

                        return result.Substring(0, 1).ToLowerInvariant() + result.Substring(1);
                    }

                    return result;
                }

                return result.ToLowerInvariant();
            }

            return result;
        }

        /// <summary>
        /// Converts string to a URL alias.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="charReplacements">The char replacements.</param>
        /// <param name="replaceDoubleDashes">if set to <c>true</c> replace double dashes.</param>
        /// <param name="stripNonAscii">if set to <c>true</c> strip non ASCII.</param>
        /// <param name="urlEncode">if set to <c>true</c> URL encode.</param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that ONLY ascii chars are allowed and of those ascii chars, only digits and lowercase chars, all
        /// punctuation, etc... are stripped out, however this method allows you to pass in string's to replace with the
        /// specified replacement character before the string is converted to ascii and it has invalid characters stripped out.
        /// This allows you to replace strings like &amp; , etc.. with your replacement character before the automatic
        /// reduction.
        /// </remarks>
        public string LegacyToUrlAlias(string value, IDictionary<string, string> charReplacements, bool replaceDoubleDashes, bool stripNonAscii, bool urlEncode)
        {
            // to lower case invariant
            // replace chars one by one using charReplacements
            // (opt) convert to ASCII then remove anything that's not ASCII
            // trim - and _ then (opt) remove double -
            // (opt) url-encode

            // charReplacement is actually *string* replacement ie it can replace "&nbsp;" by a non-breaking space
            // so it's kind of a pre-filter actually...
            // we need pre-filters, and post-filters, within each token...
            // not so... we may want to replace &nbsp; with a space BEFORE cutting into tokens...

            //first to lower case
            value = value.ToLowerInvariant();

            //then replacement chars
            value = charReplacements.Aggregate(value, (current, kvp) => current.Replace(kvp.Key, kvp.Value));

            //then convert to only ascii, this will remove the rest of any invalid chars
            if (stripNonAscii)
            {
                value = Encoding.ASCII.GetString(
                    Encoding.Convert(
                        Encoding.UTF8,
                        Encoding.GetEncoding(
                            Encoding.ASCII.EncodingName,
                            new EncoderReplacementFallback(String.Empty),
                            new DecoderExceptionFallback()),
                        Encoding.UTF8.GetBytes(value)));

                //remove all characters that do not fall into the following categories (apart from the replacement val)
                var validCodeRanges =
                    //digits
                    Enumerable.Range(48, 10).Concat(
                    //lowercase chars
                        Enumerable.Range(97, 26));

                var sb = new StringBuilder();
                foreach (var c in value.Where(c => charReplacements.Values.Contains(c.ToString(CultureInfo.InvariantCulture)) || validCodeRanges.Contains(c)))
                {
                    sb.Append(c);
                }

                value = sb.ToString();
            }

            //trim dashes from end
            value = value.Trim('-', '_');

            //replace double occurances of - or _
            value = replaceDoubleDashes ? Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled) : value;

            //url encode result
            return urlEncode ? System.Web.HttpUtility.UrlEncode(value) : value;
        }

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="url">The text to filter.</param>
        /// <returns>The safe url segment.</returns>
        /// <remarks>
        /// <para>Uses <c>UmbracoSettings.UrlReplaceCharacters</c>
        ///  and <c>UmbracoSettings.RemoveDoubleDashesFromUrlReplacing</c>.</para>
        /// </remarks>
        public string LegacyFormatUrl(string url)
        {
            var newUrl = url.ToLowerInvariant();
            foreach (var n in UmbracoConfig.For.UmbracoSettings().RequestHandler.CharCollection)
            {
                if (n.Char != "")
                    newUrl = newUrl.Replace(n.Char, n.Replacement);
            }

            // check for double dashes
            if (UmbracoConfig.For.UmbracoSettings().RequestHandler.RemoveDoubleDashes)
            {
                newUrl = Regex.Replace(newUrl, @"[-]{2,}", "-");
            }

            return newUrl;
        }

        #endregion

        #region ReplaceMany

        /// <summary>
        /// Returns a new string in which all occurences of specified strings are replaced by other specified strings.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="replacements">The replacements definition.</param>
        /// <returns>The filtered string.</returns>
        public string ReplaceMany(string text, IDictionary<string, string> replacements)
        {
            // Have done various tests, implementing my own "super fast" state machine to handle 
            // replacement of many items, or via regexes, but on short strings and not too
            // many replacements (which prob. is going to be our case) nothing can beat this...
            // (at least with safe and checked code -- we don't want unsafe/unchecked here)

            // Note that it will do chained-replacements ie replaced items can be replaced
            // in turn by another replacement (ie the order of replacements is important)

            return replacements.Aggregate(text, (current, kvp) => current.Replace(kvp.Key, kvp.Value));
        }

        /// <summary>
        /// Returns a new string in which all occurences of specified characters are replaced by a specified character.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="chars">The characters to replace.</param>
        /// <param name="replacement">The replacement character.</param>
        /// <returns>The filtered string.</returns>
        public string ReplaceMany(string text, char[] chars, char replacement)
        {
            // be safe
            if (text == null)
                throw new ArgumentNullException("text");
            if (chars == null)
                throw new ArgumentNullException("chars");

            // see note above

            return chars.Aggregate(text, (current, c) => current.Replace(c, replacement));
        }

        #endregion
    }
}
