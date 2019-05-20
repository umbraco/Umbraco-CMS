using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Security;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Strings;

namespace Umbraco.Core
{
    public static class StringExtensions
    {
        /// <summary>
        /// Based on the input string, this will detect if the string is a JS path or a JS snippet.
        /// If a path cannot be determined, then it is assumed to be a snippet the original text is returned
        /// with an invalid attempt, otherwise a valid attempt is returned with the resolved path
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is only used for legacy purposes for the Action.JsSource stuff and shouldn't be needed in v8
        /// </remarks>
        internal static Attempt<string> DetectIsJavaScriptPath(this string input)
        {
            //validate that this is a url, if it is not, we'll assume that it is a text block and render it as a text
            //block instead.
            var isValid = true;

            if (Uri.IsWellFormedUriString(input, UriKind.RelativeOrAbsolute))
            {
                //ok it validates, but so does alert('hello'); ! so we need to do more checks

                //here are the valid chars in a url without escaping
                if (Regex.IsMatch(input, @"[^a-zA-Z0-9-._~:/?#\[\]@!$&'\(\)*\+,%;=]"))
                    isValid = false;

                //we'll have to be smarter and just check for certain js patterns now too!
                var jsPatterns = new[] { @"\+\s*\=", @"\);", @"function\s*\(", @"!=", @"==" };
                if (jsPatterns.Any(p => Regex.IsMatch(input, p)))
                    isValid = false;

                if (isValid)
                {
                    var resolvedUrlResult = IOHelper.TryResolveUrl(input);
                    //if the resolution was success, return it, otherwise just return the path, we've detected
                    // it's a path but maybe it's relative and resolution has failed, etc... in which case we're just
                    // returning what was given to us.
                    return resolvedUrlResult.Success
                        ? resolvedUrlResult
                        : Attempt.Succeed(input);
                }
            }

            return Attempt.Fail(input);
        }

        /// <summary>
        /// Returns a JObject/JArray instance if the string can be converted to json, otherwise returns the string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static object ConvertToJsonIfPossible(this string input)
        {
            if (input.DetectIsJson() == false)
            {
                return input;
            }
            try
            {
                var obj = JsonConvert.DeserializeObject(input);
                return obj;
            }
            catch (Exception)
            {
                return input;
            }
        }

        /// <summary>
        /// Encrypt the string using the MachineKey in medium trust
        /// </summary>
        /// <param name="value">The string value to be encrypted.</param>
        /// <returns>The encrypted string.</returns>
        public static string EncryptWithMachineKey(this string value)
        {
            if (value == null)
                return null;

            string valueToEncrypt = value;
            List<string> parts = new List<string>();

            const int EncrpytBlockSize = 500;

            while (valueToEncrypt.Length > EncrpytBlockSize)
            {
                parts.Add(valueToEncrypt.Substring(0, EncrpytBlockSize));
                valueToEncrypt = valueToEncrypt.Remove(0, EncrpytBlockSize);
            }

            if (valueToEncrypt.Length > 0)
            {
                parts.Add(valueToEncrypt);
            }

            StringBuilder encrpytedValue = new StringBuilder();

            foreach (var part in parts)
            {
                var encrpytedBlock = FormsAuthentication.Encrypt(new FormsAuthenticationTicket(0, string.Empty, DateTime.Now, DateTime.MaxValue, false, part));
                encrpytedValue.AppendLine(encrpytedBlock);
            }

            return encrpytedValue.ToString().TrimEnd();
        }

        /// <summary>
        /// Decrypt the encrypted string using the Machine key in medium trust
        /// </summary>
        /// <param name="value">The string value to be decrypted</param>
        /// <returns>The decrypted string.</returns>
        public static string DecryptWithMachineKey(this string value)
        {
            if (value == null)
                return null;

            string[] parts = value.Split('\n');

            StringBuilder decryptedValue = new StringBuilder();

            foreach (var part in parts)
            {
                decryptedValue.Append(FormsAuthentication.Decrypt(part.TrimEnd()).UserData);
            }

            return decryptedValue.ToString();
        }

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
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullOrEmptyException(nameof(text));

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
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullOrEmptyException(nameof(text));

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
