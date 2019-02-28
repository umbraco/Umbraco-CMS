﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Strings;

namespace Umbraco.Core
{

    ///<summary>
    /// String extension methods
    ///</summary>
    public static class StringExtensions
    {

        private static readonly char[] ToCSharpHexDigitLower = "0123456789abcdef".ToCharArray();
        private static readonly char[] ToCSharpEscapeChars;

        static StringExtensions()
        {
            var escapes = new[] { "\aa", "\bb", "\ff", "\nn", "\rr", "\tt", "\vv", "\"\"", "\\\\", "??", "\00" };
            ToCSharpEscapeChars = new char[escapes.Max(e => e[0]) + 1];
            foreach (var escape in escapes)
                ToCSharpEscapeChars[escape[0]] = escape[1];
        }

        /// <summary>
        /// Convert a path to node ids in the order from right to left (deepest to shallowest)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static int[] GetIdsFromPathReversed(this string path)
        {
            var nodeIds = path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.TryConvertTo<int>())
                .Where(x => x.Success)
                .Select(x => x.Result)
                .Reverse()
                .ToArray();
            return nodeIds;
        }

        /// <summary>
        /// Removes new lines and tabs
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        internal static string StripWhitespace(this string txt)
        {
            return Regex.Replace(txt, @"\s", string.Empty);
        }

        internal static string StripFileExtension(this string fileName)
        {
            //filenames cannot contain line breaks
            if (fileName.Contains(Environment.NewLine) || fileName.Contains("\r") || fileName.Contains("\n")) return fileName;

            var lastIndex = fileName.LastIndexOf('.');
            if (lastIndex > 0)
            {
                var ext = fileName.Substring(lastIndex);
                //file extensions cannot contain whitespace
                if (ext.Contains(" ")) return fileName;

                return string.Format("{0}", fileName.Substring(0, fileName.IndexOf(ext, StringComparison.Ordinal)));
            }
            return fileName;


        }

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
        /// This tries to detect a json string, this is not a fail safe way but it is quicker than doing
        /// a try/catch when deserializing when it is not json.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool DetectIsJson(this string input)
        {
            if (input.IsNullOrWhiteSpace()) return false;
            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}"))
                   || (input.StartsWith("[") && input.EndsWith("]"));
        }

        internal static readonly Lazy<Regex> Whitespace = new Lazy<Regex>(() => new Regex(@"\s+", RegexOptions.Compiled));
        internal static readonly string[] JsonEmpties = { "[]", "{}" };
        internal static bool DetectIsEmptyJson(this string input)
        {
            return JsonEmpties.Contains(Whitespace.Value.Replace(input, string.Empty));
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

        internal static string ReplaceNonAlphanumericChars(this string input, string replacement)
        {
            //any character that is not alphanumeric, convert to a hyphen
            var mName = input;
            foreach (var c in mName.ToCharArray().Where(c => !char.IsLetterOrDigit(c)))
            {
                mName = mName.Replace(c.ToString(CultureInfo.InvariantCulture), replacement);
            }
            return mName;
        }

        internal static string ReplaceNonAlphanumericChars(this string input, char replacement)
        {
            var inputArray = input.ToCharArray();
            var outputArray = new char[input.Length];
            for (var i = 0; i < inputArray.Length; i++)
                outputArray[i] = char.IsLetterOrDigit(inputArray[i]) ? inputArray[i] : replacement;
            return new string(outputArray);
        }
        private static readonly char[] CleanForXssChars = "*?(){}[];:%<>/\\|&'\"".ToCharArray();

        /// <summary>
        /// Cleans string to aid in preventing xss attacks.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ignoreFromClean"></param>
        /// <returns></returns>
        public static string CleanForXss(this string input, params char[] ignoreFromClean)
        {
            //remove any HTML
            input = input.StripHtml();
            //strip out any potential chars involved with XSS
            return input.ExceptChars(new HashSet<char>(CleanForXssChars.Except(ignoreFromClean)));
        }

        public static string ExceptChars(this string str, HashSet<char> toExclude)
        {
            var sb = new StringBuilder(str.Length);
            foreach (var c in str.Where(c => toExclude.Contains(c) == false))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a stream from a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static Stream GenerateStreamFromString(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// This will append the query string to the URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        /// <remarks>
        /// This methods ensures that the resulting URL is structured correctly, that there's only one '?' and that things are
        /// delimited properly with '&'
        /// </remarks>
        internal static string AppendQueryStringToUrl(this string url, params string[] queryStrings)
        {
            //remove any prefixed '&' or '?'
            for (var i = 0; i < queryStrings.Length; i++)
            {
                queryStrings[i] = queryStrings[i].TrimStart('?', '&').TrimEnd('&');
            }

            var nonEmpty = queryStrings.Where(x => !x.IsNullOrWhiteSpace()).ToArray();

            if (url.Contains("?"))
            {
                return url + string.Join("&", nonEmpty).EnsureStartsWith('&');
            }
            return url + string.Join("&", nonEmpty).EnsureStartsWith('?');
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

        //this is from SqlMetal and just makes it a bit of fun to allow pluralization
        public static string MakePluralName(this string name)
        {
            if ((name.EndsWith("x", StringComparison.OrdinalIgnoreCase) || name.EndsWith("ch", StringComparison.OrdinalIgnoreCase)) || (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) || name.EndsWith("sh", StringComparison.OrdinalIgnoreCase)))
            {
                name = name + "es";
                return name;
            }
            if ((name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && (name.Length > 1)) && !IsVowel(name[name.Length - 2]))
            {
                name = name.Remove(name.Length - 1, 1);
                name = name + "ies";
                return name;
            }
            if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                name = name + "s";
            }
            return name;
        }

        public static bool IsVowel(this char c)
        {
            switch (c)
            {
                case 'O':
                case 'U':
                case 'Y':
                case 'A':
                case 'E':
                case 'I':
                case 'o':
                case 'u':
                case 'y':
                case 'a':
                case 'e':
                case 'i':
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Trims the specified value from a string; accepts a string input whereas the in-built implementation only accepts char or char[].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="forRemoving">For removing.</param>
        /// <returns></returns>
        public static string Trim(this string value, string forRemoving)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.TrimEnd(forRemoving).TrimStart(forRemoving);
        }

        public static string EncodeJsString(this string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        public static string TrimEnd(this string value, string forRemoving)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (string.IsNullOrEmpty(forRemoving)) return value;

            while (value.EndsWith(forRemoving, StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Remove(value.LastIndexOf(forRemoving, StringComparison.InvariantCultureIgnoreCase));
            }
            return value;
        }

        public static string TrimStart(this string value, string forRemoving)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (string.IsNullOrEmpty(forRemoving)) return value;

            while (value.StartsWith(forRemoving, StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(forRemoving.Length);
            }
            return value;
        }

        public static string EnsureStartsWith(this string input, string toStartWith)
        {
            if (input.StartsWith(toStartWith)) return input;
            return toStartWith + input.TrimStart(toStartWith);
        }

        public static string EnsureStartsWith(this string input, char value)
        {
            return input.StartsWith(value.ToString(CultureInfo.InvariantCulture)) ? input : value + input;
        }

        public static string EnsureEndsWith(this string input, char value)
        {
            return input.EndsWith(value.ToString(CultureInfo.InvariantCulture)) ? input : input + value;
        }

        public static string EnsureEndsWith(this string input, string toEndWith)
        {
            return input.EndsWith(toEndWith.ToString(CultureInfo.InvariantCulture)) ? input : input + toEndWith;
        }

        public static bool IsLowerCase(this char ch)
        {
            return ch.ToString(CultureInfo.InvariantCulture) == ch.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
        }

        public static bool IsUpperCase(this char ch)
        {
            return ch.ToString(CultureInfo.InvariantCulture) == ch.ToString(CultureInfo.InvariantCulture).ToUpperInvariant();
        }

        /// <summary>Indicates whether a specified string is null, empty, or
        /// consists only of white-space characters.</summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Returns <see langword="true"/> if the value is null,
        /// empty, or consists only of white-space characters, otherwise
        /// returns <see langword="false"/>.</returns>
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static string IfNullOrWhiteSpace(this string str, string defaultValue)
        {
            return str.IsNullOrWhiteSpace() ? defaultValue : str;
        }

        /// <summary>The to delimited list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>the list</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "By design")]
        public static IList<string> ToDelimitedList(this string list, string delimiter = ",")
        {
            var delimiters = new[] { delimiter };
            return !list.IsNullOrWhiteSpace()
                       ? list.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                             .Select(i => i.Trim())
                             .ToList()
                       : new List<string>();
        }

        /// <summary>enum try parse.</summary>
        /// <param name="strType">The str type.</param>
        /// <param name="ignoreCase">The ignore case.</param>
        /// <param name="result">The result.</param>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>The enum try parse.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By Design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By Design")]
        public static bool EnumTryParse<T>(this string strType, bool ignoreCase, out T result)
        {
            try
            {
                result = (T)Enum.Parse(typeof(T), strType, ignoreCase);
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Parse string to Enum
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="strType">The string to parse</param>
        /// <param name="ignoreCase">The ignore case</param>
        /// <returns>The parsed enum</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By Design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By Design")]
        public static T EnumParse<T>(this string strType, bool ignoreCase)
        {
            return (T)Enum.Parse(typeof(T), strType, ignoreCase);
        }

        /// <summary>
        /// Strips all HTML from a string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Returns the string without any HTML tags.</returns>
        public static string StripHtml(this string text)
        {
            const string pattern = @"<(.|\n)*?>";
            return Regex.Replace(text, pattern, string.Empty, RegexOptions.Compiled);
        }

        /// <summary>
        /// Encodes as GUID.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Guid EncodeAsGuid(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentNullException("input");

            var convertToHex = input.ConvertToHex();
            var hexLength = convertToHex.Length < 32 ? convertToHex.Length : 32;
            var hex = convertToHex.Substring(0, hexLength).PadLeft(32, '0');
            var output = Guid.Empty;
            return Guid.TryParse(hex, out output) ? output : Guid.Empty;
        }

        /// <summary>
        /// Converts to hex.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string ConvertToHex(this string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                sb.AppendFormat("{0:x2}", Convert.ToUInt32(c));
            }
            return sb.ToString();
        }

        public static string DecodeFromHex(this string hexValue)
        {
            var strValue = "";
            while (hexValue.Length > 0)
            {
                strValue += Convert.ToChar(Convert.ToUInt32(hexValue.Substring(0, 2), 16)).ToString();
                hexValue = hexValue.Substring(2, hexValue.Length - 2);
            }
            return strValue;
        }

        ///<summary>
        /// Encodes a string to a safe URL base64 string
        ///</summary>
        ///<param name="input"></param>
        ///<returns></returns>
        public static string ToUrlBase64(this string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrEmpty(input))
                return string.Empty;

            //return Convert.ToBase64String(bytes).Replace(".", "-").Replace("/", "_").Replace("=", ",");
            var bytes = Encoding.UTF8.GetBytes(input);
            return UrlTokenEncode(bytes);
        }

        /// <summary>
        /// Decodes a URL safe base64 string back
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FromUrlBase64(this string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            //if (input.IsInvalidBase64()) return null;

            try
            {
                //var decodedBytes = Convert.FromBase64String(input.Replace("-", ".").Replace("_", "/").Replace(",", "="));
                var decodedBytes = UrlTokenDecode(input);
                return decodedBytes != null ? Encoding.UTF8.GetString(decodedBytes) : null;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// formats the string with invariant culture
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static string InvariantFormat(this string format, params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Converts an integer to an invariant formatted string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToInvariantString(this int str)
        {
            return str.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this long str)
        {
            return str.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Compares 2 strings with invariant culture and case ignored
        /// </summary>
        /// <param name="compare">The compare.</param>
        /// <param name="compareTo">The compare to.</param>
        /// <returns></returns>
        public static bool InvariantEquals(this string compare, string compareTo)
        {
            return String.Equals(compare, compareTo, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool InvariantStartsWith(this string compare, string compareTo)
        {
            return compare.StartsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool InvariantEndsWith(this string compare, string compareTo)
        {
            return compare.EndsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool InvariantContains(this string compare, string compareTo)
        {
            return compare.IndexOf(compareTo, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool InvariantContains(this IEnumerable<string> compare, string compareTo)
        {
            return compare.Contains(compareTo, StringComparer.InvariantCultureIgnoreCase);
        }

        public static int InvariantIndexOf(this string s, string value)
        {
            return s.IndexOf(value, StringComparison.OrdinalIgnoreCase);
        }

        public static int InvariantLastIndexOf(this string s, string value)
        {
            return s.LastIndexOf(value, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Tries to parse a string into the supplied type by finding and using the Type's "Parse" method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static T ParseInto<T>(this string val)
        {
            return (T)val.ParseInto(typeof(T));
        }

        /// <summary>
        /// Tries to parse a string into the supplied type by finding and using the Type's "Parse" method
        /// </summary>
        /// <param name="val"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ParseInto(this string val, Type type)
        {
            if (string.IsNullOrEmpty(val) == false)
            {
                TypeConverter tc = TypeDescriptor.GetConverter(type);
                return tc.ConvertFrom(val);
            }
            return val;
        }

        /// <summary>
        /// Generates a hash of a string based on the FIPS compliance setting.
        /// </summary>
        /// <param name="str">Refers to itself</param>
        /// <returns>The hashed string</returns>
        public static string GenerateHash(this string str)
        {
            return CryptoConfig.AllowOnlyFipsAlgorithms
                ? str.ToSHA1()
                : str.ToMd5();
        }

        /// <summary>
        /// Converts the string to MD5
        /// </summary>
        /// <param name="stringToConvert">Refers to itself</param>
        /// <returns>The MD5 hashed string</returns>
        [Obsolete("Please use the GenerateHash method instead. This may be removed in future versions")]
        internal static string ToMd5(this string stringToConvert)
        {
            return stringToConvert.GenerateHash("MD5");
        }

        /// <summary>
        /// Converts the string to SHA1
        /// </summary>
        /// <param name="stringToConvert">refers to itself</param>
        /// <returns>The SHA1 hashed string</returns>
        [Obsolete("Please use the GenerateHash method instead. This may be removed in future versions")]
        internal static string ToSHA1(this string stringToConvert)
        {
            return stringToConvert.GenerateHash("SHA1");
        }

        /// <summary>Generate a hash of a string based on the hashType passed in
        /// </summary>
        /// <param name="str">Refers to itself</param>
        /// <param name="hashType">String with the hash type.  See remarks section of the CryptoConfig Class in MSDN docs for a list of possible values.</param>
        /// <returns>The hashed string</returns>
        private static string GenerateHash(this string str, string hashType)
        {
            //create an instance of the correct hashing provider based on the type passed in
            var hasher = HashAlgorithm.Create(hashType);
            if (hasher == null) throw new InvalidOperationException("No hashing type found by name " + hashType);
            using (hasher)
            {
                //convert our string into byte array
                var byteArray = Encoding.UTF8.GetBytes(str);

                //get the hashed values created by our selected provider
                var hashedByteArray = hasher.ComputeHash(byteArray);

                //create a StringBuilder object
                var stringBuilder = new StringBuilder();

                //loop to each byte
                foreach (var b in hashedByteArray)
                {
                    //append it to our StringBuilder
                    stringBuilder.Append(b.ToString("x2"));
                }

                //return the hashed value
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Decodes a string that was encoded with UrlTokenEncode
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static byte[] UrlTokenDecode(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Length == 0)
                return Array.Empty<byte>();

            // calc array size - must be groups of 4
            var arrayLength = input.Length;
            var remain = arrayLength % 4;
            if (remain != 0) arrayLength += 4 - remain;

            var inArray = new char[arrayLength];
            for (var i = 0; i < input.Length; i++)
            {
                var ch = input[i];
                switch (ch)
                {
                    case '-': // restore '-' as '+'
                        inArray[i] = '+';
                        break;

                    case '_': // restore '_' as '/'
                        inArray[i] = '/';
                        break;

                    default: // keep char unchanged
                        inArray[i] = ch;
                        break;
                }
            }

            // pad with '='
            for (var j = input.Length; j < inArray.Length; j++)
                inArray[j] = '=';

            return Convert.FromBase64CharArray(inArray, 0, inArray.Length);
        }

        /// <summary>
        /// Encodes a string so that it is 'safe' for URLs, files, etc..
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string UrlTokenEncode(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Length == 0)
                return string.Empty;

            // base-64 digits are A-Z, a-z, 0-9, + and /
            // the = char is used for trailing padding

            var str = Convert.ToBase64String(input);

            var pos = str.IndexOf('=');
            if (pos < 0) pos = str.Length;

            // replace chars that would cause problems in urls
            var chArray = new char[pos];
            for (var i = 0; i < pos; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '+': // replace '+' with '-'
                        chArray[i] = '-';
                        break;

                    case '/': // replace '/' with '_'
                        chArray[i] = '_';
                        break;

                    default: // keep char unchanged
                        chArray[i] = ch;
                        break;
                }
            }

            return new string(chArray);
        }

        /// <summary>
        /// Ensures that the folder path ends with a DirectorySeparatorChar
        /// </summary>
        /// <param name="currentFolder"></param>
        /// <returns></returns>
        public static string NormaliseDirectoryPath(this string currentFolder)
        {
            currentFolder = currentFolder
                                .IfNull(x => String.Empty)
                                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return currentFolder;
        }

        /// <summary>
        /// Truncates the specified text string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="maxLength">Length of the max.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns></returns>
        public static string Truncate(this string text, int maxLength, string suffix = "...")
        {
            // replaces the truncated string to a ...
            var truncatedString = text;

            if (maxLength <= 0) return truncatedString;
            var strLength = maxLength - suffix.Length;

            if (strLength <= 0) return truncatedString;

            if (text == null || text.Length <= maxLength) return truncatedString;

            truncatedString = text.Substring(0, strLength);
            truncatedString = truncatedString.TrimEnd();
            truncatedString += suffix;

            return truncatedString;
        }

        /// <summary>
        /// Strips carrage returns and line feeds from the specified text.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string StripNewLines(this string input)
        {
            return input.Replace("\r", "").Replace("\n", "");
        }

        /// <summary>
        /// Converts to single line by replacing line breaks with spaces.
        /// </summary>
        public static string ToSingleLine(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = text.Replace("\r\n", " "); // remove CRLF
            text = text.Replace("\r", " "); // remove CR
            text = text.Replace("\n", " "); // remove LF
            return text;
        }

        public static string OrIfNullOrWhiteSpace(this string input, string alternative)
        {
            return !string.IsNullOrWhiteSpace(input)
                       ? input
                       : alternative;
        }

        /// <summary>
        /// Returns a copy of the string with the first character converted to uppercase.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <returns>The converted string.</returns>
        public static string ToFirstUpper(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        /// <summary>
        /// Returns a copy of the string with the first character converted to lowercase.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <returns>The converted string.</returns>
        public static string ToFirstLower(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : input.Substring(0, 1).ToLower() + input.Substring(1);
        }

        /// <summary>
        /// Returns a copy of the string with the first character converted to uppercase using the casing rules of the specified culture.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The converted string.</returns>
        public static string ToFirstUpper(this string input, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : input.Substring(0, 1).ToUpper(culture) + input.Substring(1);
        }

        /// <summary>
        /// Returns a copy of the string with the first character converted to lowercase using the casing rules of the specified culture.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The converted string.</returns>
        public static string ToFirstLower(this string input, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : input.Substring(0, 1).ToLower(culture) + input.Substring(1);
        }

        /// <summary>
        /// Returns a copy of the string with the first character converted to uppercase using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <returns>The converted string.</returns>
        public static string ToFirstUpperInvariant(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : input.Substring(0, 1).ToUpperInvariant() + input.Substring(1);
        }

        /// <summary>
        /// Returns a copy of the string with the first character converted to lowercase using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <returns>The converted string.</returns>
        public static string ToFirstLowerInvariant(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : input.Substring(0, 1).ToLowerInvariant() + input.Substring(1);
        }

        /// <summary>
        /// Returns a new string in which all occurrences of specified strings are replaced by other specified strings.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="replacements">The replacements definition.</param>
        /// <returns>The filtered string.</returns>
        public static string ReplaceMany(this string text, IDictionary<string, string> replacements)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (replacements == null) throw new ArgumentNullException(nameof(replacements));


            foreach (KeyValuePair<string, string> item in replacements)
                text = text.Replace(item.Key, item.Value);

            return text;
        }

        /// <summary>
        /// Returns a new string in which all occurrences of specified characters are replaced by a specified character.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="chars">The characters to replace.</param>
        /// <param name="replacement">The replacement character.</param>
        /// <returns>The filtered string.</returns>
        public static string ReplaceMany(this string text, char[] chars, char replacement)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (chars == null) throw new ArgumentNullException(nameof(chars));


            for (int i = 0; i < chars.Length; i++)
                text = text.Replace(chars[i], replacement);

            return text;
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

        /// <summary>
        /// An extension method that returns a new string in which all occurrences of a
        /// specified string in the current instance are replaced with another specified string.
        /// StringComparison specifies the type of search to use for the specified string.
        /// </summary>
        /// <param name="source">Current instance of the string</param>
        /// <param name="oldString">Specified string to replace</param>
        /// <param name="newString">Specified string to inject</param>
        /// <param name="stringComparison">String Comparison object to specify search type</param>
        /// <returns>Updated string</returns>
        public static string Replace(this string source, string oldString, string newString, StringComparison stringComparison)
        {
            // This initialization ensures the first check starts at index zero of the source. On successive checks for
            // a match, the source is skipped to immediately after the last replaced occurrence for efficiency
            // and to avoid infinite loops when oldString and newString compare equal.
            int index = -1 * newString.Length;

            // Determine if there are any matches left in source, starting from just after the result of replacing the last match.
            while ((index = source.IndexOf(oldString, index + newString.Length, stringComparison)) >= 0)
            {
                // Remove the old text.
                source = source.Remove(index, oldString.Length);

                // Add the replacement text.
                source = source.Insert(index, newString);
            }

            return source;
        }

        /// <summary>
        /// Converts a literal string into a C# expression.
        /// </summary>
        /// <param name="s">Current instance of the string.</param>
        /// <returns>The string in a C# format.</returns>
        public static string ToCSharpString(this string s)
        {
            if (s == null) return "<null>";

            // http://stackoverflow.com/questions/323640/can-i-convert-a-c-sharp-string-value-to-an-escaped-string-literal

            var sb = new StringBuilder(s.Length + 2);
            for (var rp = 0; rp < s.Length; rp++)
            {
                var c = s[rp];
                if (c < ToCSharpEscapeChars.Length && '\0' != ToCSharpEscapeChars[c])
                    sb.Append('\\').Append(ToCSharpEscapeChars[c]);
                else if ('~' >= c && c >= ' ')
                    sb.Append(c);
                else
                    sb.Append(@"\x")
                      .Append(ToCSharpHexDigitLower[c >> 12 & 0x0F])
                      .Append(ToCSharpHexDigitLower[c >> 8 & 0x0F])
                      .Append(ToCSharpHexDigitLower[c >> 4 & 0x0F])
                      .Append(ToCSharpHexDigitLower[c & 0x0F]);
            }

            return sb.ToString();

            // requires full trust
            /*
            using (var writer = new StringWriter())
            using (var provider = CodeDomProvider.CreateProvider("CSharp"))
            {
                provider.GenerateCodeFromExpression(new CodePrimitiveExpression(s), writer, null);
                return writer.ToString().Replace(string.Format("\" +{0}\t\"", Environment.NewLine), "");
            }
            */
        }

        public static string EscapeRegexSpecialCharacters(this string text)
        {
            var regexSpecialCharacters = new Dictionary<string, string>
            {
                {".", @"\."},
                {"(", @"\("},
                {")", @"\)"},
                {"]", @"\]"},
                {"[", @"\["},
                {"{", @"\{"},
                {"}", @"\}"},
                {"?", @"\?"},
                {"!", @"\!"},
                {"$", @"\$"},
                {"^", @"\^"},
                {"+", @"\+"},
                {"*", @"\*"},
                {"|", @"\|"},
                {"<", @"\<"},
                {">", @"\>"}
            };
            return ReplaceMany(text, regexSpecialCharacters);
        }

        /// <summary>
        /// Checks whether a string "haystack" contains within it any of the strings in the "needles" collection and returns true if it does or false if it doesn't
        /// </summary>
        /// <param name="haystack">The string to check</param>
        /// <param name="needles">The collection of strings to check are contained within the first string</param>
        /// <param name="comparison">The type of comparison to perform - defaults to <see cref="StringComparison.CurrentCulture"/></param>
        /// <returns>True if any of the needles are contained with haystack; otherwise returns false</returns>
        /// Added fix to ensure the comparison is used - see http://issues.umbraco.org/issue/U4-11313
        public static bool ContainsAny(this string haystack, IEnumerable<string> needles, StringComparison comparison = StringComparison.CurrentCulture)
        {
            if (haystack == null)
                throw new ArgumentNullException("haystack");

            if (string.IsNullOrEmpty(haystack) || needles == null || !needles.Any())
            {
                return false;
            }

            return needles.Any(value => haystack.IndexOf(value, comparison) >= 0);
        }

        public static bool CsvContains(this string csv, string value)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return false;
            }
            var idCheckList = csv.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            return idCheckList.Contains(value);
        }

        /// <summary>
        /// Converts a file name to a friendly name for a content item
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ToFriendlyName(this string fileName)
        {
            // strip the file extension
            fileName = fileName.StripFileExtension();

            // underscores and dashes to spaces
            fileName = fileName.ReplaceMany(new[] { '_', '-' }, ' ');

            // any other conversions ?

            // Pascalcase (to be done last)
            fileName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fileName);

            // Replace multiple consecutive spaces with a single space
            fileName = string.Join(" ", fileName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            return fileName;
        }

        // From: http://stackoverflow.com/a/961504/5018
        // filters control characters but allows only properly-formed surrogate sequences
        private static readonly Lazy<Regex> InvalidXmlChars = new Lazy<Regex>(() =>
            new Regex(
                @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
                RegexOptions.Compiled));


        /// <summary>
        /// An extension method that returns a new string in which all occurrences of an
        /// unicode characters that are invalid in XML files are replaced with an empty string.
        /// </summary>
        /// <param name="text">Current instance of the string</param>
        /// <returns>Updated string</returns>
        ///
        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        internal static string ToValidXmlString(this string text)
        {
            return string.IsNullOrEmpty(text) ? text : InvalidXmlChars.Value.Replace(text, "");
        }

        /// <summary>
        /// Converts a string to a Guid - WARNING, depending on the string, this may not be unique
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static Guid ToGuid(this string text)
        {
            return CreateGuidFromHash(UrlNamespace,
                                        text,
                                        CryptoConfig.AllowOnlyFipsAlgorithms
                                            ? 5     // SHA1
                                            : 3);   // MD5
        }

        /// <summary>
        /// The namespace for URLs (from RFC 4122, Appendix C).
        ///
        /// See <a href="http://www.ietf.org/rfc/rfc4122.txt">RFC 4122</a>
        /// </summary>
        internal static readonly Guid UrlNamespace = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

        /// <summary>
        /// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
        ///
        /// See <a href="https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/GuidUtility.cs#L34">GuidUtility.cs</a> for original implementation.
        /// </summary>
        /// <param name="namespaceId">The ID of the namespace.</param>
        /// <param name="name">The name (within that namespace).</param>
        /// <param name="version">The version number of the UUID to create; this value must be either
        /// 3 (for MD5 hashing) or 5 (for SHA-1 hashing).</param>
        /// <returns>A UUID derived from the namespace and name.</returns>
        /// <remarks>See <a href="http://code.logos.com/blog/2011/04/generating_a_deterministic_guid.html">Generating a deterministic GUID</a>.</remarks>
        internal static Guid CreateGuidFromHash(Guid namespaceId, string name, int version)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (version != 3 && version != 5)
                throw new ArgumentOutOfRangeException("version", "version must be either 3 or 5.");

            // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
            // ASSUME: UTF-8 encoding is always appropriate
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);

            // convert the namespace UUID to network order (step 3)
            byte[] namespaceBytes = namespaceId.ToByteArray();
            SwapByteOrder(namespaceBytes);

            // comput the hash of the name space ID concatenated with the name (step 4)
            byte[] hash;
            using (HashAlgorithm algorithm = version == 3 ? (HashAlgorithm)MD5.Create() : SHA1.Create())
            {
                algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
                algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
                hash = algorithm.Hash;
            }

            // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
            byte[] newGuid = new byte[16];
            Array.Copy(hash, 0, newGuid, 0, 16);

            // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
            newGuid[6] = (byte)((newGuid[6] & 0x0F) | (version << 4));

            // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
            newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

            // convert the resulting UUID to local byte order (step 13)
            SwapByteOrder(newGuid);
            return new Guid(newGuid);
        }

        // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
        internal static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        private static void SwapBytes(byte[] guid, int left, int right)
        {
            byte temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
        }

        /// <summary>
        /// Turns an null-or-whitespace string into a null string.
        /// </summary>
        public static string NullOrWhiteSpaceAsNull(this string text)
            => string.IsNullOrWhiteSpace(text) ? null : text;
    }
}
