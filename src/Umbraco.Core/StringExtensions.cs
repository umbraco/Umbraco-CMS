using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using System.Web.Security;
using Umbraco.Core.Strings;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core
{

    ///<summary>
    /// String extension methods
    ///</summary>
    public static class StringExtensions
    {
        [UmbracoWillObsolete("Do not use this constants. See IShortStringHelper.CleanStringForSafeAliasJavaScriptCode.")]
        public const string UmbracoValidAliasCharacters = "_-abcdefghijklmnopqrstuvwxyz1234567890";
        [UmbracoWillObsolete("Do not use this constants. See IShortStringHelper.CleanStringForSafeAliasJavaScriptCode.")]
        public const string UmbracoInvalidFirstCharacters = "01234567890";

        private static readonly char[] ToCSharpHexDigitLower = "0123456789abcdef".ToCharArray();
        private static readonly char[] ToCSharpEscapeChars;

        static StringExtensions()
        {
            var escapes = new[] { "\aa", "\bb", "\ff", "\nn", "\rr", "\tt", "\vv", "\"\"", "\\\\", "??", "\00" };
            ToCSharpEscapeChars = new char[escapes.Max(e => e[0]) + 1];
            foreach (var escape in escapes)
                ToCSharpEscapeChars[escape[0]] = escape[1];
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
        /// This tries to detect a json string, this is not a fail safe way but it is quicker than doing 
        /// a try/catch when deserializing when it is not json.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static bool DetectIsJson(this string input)
        {
            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}"))
                   || (input.StartsWith("[") && input.EndsWith("]"));
        }

        internal static readonly Regex Whitespace = new Regex(@"\s+", RegexOptions.Compiled);
        internal static readonly string[] JsonEmpties = new [] { "[]", "{}" };
        internal static bool DetectIsEmptyJson(this string input)
        {
            return JsonEmpties.Contains(Whitespace.Replace(input, string.Empty));
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
            catch (Exception ex)
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

        /// <summary>
        /// Cleans string to aid in preventing xss attacks.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string CleanForXss(this string input)
        {
            //remove any html
            input = input.StripHtml();
            //strip out any potential chars involved with XSS
            return input.ExceptChars(new HashSet<char>("*?(){}[];:%<>/\\|&'\"".ToCharArray()));
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

        //this is from SqlMetal and just makes it a bit of fun to allow pluralisation
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

        /// <summary>Is null or white space.</summary>
        /// <param name="str">The str.</param>
        /// <returns>The is null or white space.</returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return (str == null) || (str.Trim().Length == 0);
        }

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
        /// Strips all html from a string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Returns the string without any html tags.</returns>
        public static string StripHtml(this string text)
        {
            const string pattern = @"<(.|\n)*?>";
            return Regex.Replace(text, pattern, String.Empty);
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
            if (input == null) throw new ArgumentNullException("input");

            if (String.IsNullOrEmpty(input)) return String.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            return UrlTokenEncode(bytes);
            //return Convert.ToBase64String(bytes).Replace(".", "-").Replace("/", "_").Replace("=", ",");
        }

        /// <summary>
        /// Decodes a URL safe base64 string back
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FromUrlBase64(this string input)
        {
            if (input == null) throw new ArgumentNullException("input");

            //if (input.IsInvalidBase64()) return null;

            try
            {
                //var decodedBytes = Convert.FromBase64String(input.Replace("-", ".").Replace("_", "/").Replace(",", "="));
                byte[] decodedBytes = UrlTokenDecode(input);
                return decodedBytes != null ? Encoding.UTF8.GetString(decodedBytes) : null;
            }
            catch (FormatException ex)
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

        /// <summary>
        /// Determines if the string is a Guid
        /// </summary>
        /// <param name="str"></param>
        /// <param name="withHyphens"></param>
        /// <returns></returns>
        public static bool IsGuid(this string str, bool withHyphens)
        {
            var isGuid = false;

            if (!String.IsNullOrEmpty(str))
            {
                Regex guidRegEx;
                if (withHyphens)
                {
                    guidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$");
                }
                else
                {
                    guidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}([0-9a-fA-F]){4}([0-9a-fA-F]){4}([0-9a-fA-F]){4}([0-9a-fA-F]){12}\}{0,1})$");
                }
                isGuid = guidRegEx.IsMatch(str);
            }

            return isGuid;
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
            if (!String.IsNullOrEmpty(val))
            {
                TypeConverter tc = TypeDescriptor.GetConverter(type);
                return tc.ConvertFrom(val);
            }
            return val;
        }

        /// <summary>
        /// Converts the string to MD5
        /// </summary>
        /// <param name="stringToConvert">referrs to itself</param>
        /// <returns>the md5 hashed string</returns>
        public static string ToMd5(this string stringToConvert)
        {
            //create an instance of the MD5CryptoServiceProvider
            var md5Provider = new MD5CryptoServiceProvider();

            //convert our string into byte array
            var byteArray = Encoding.UTF8.GetBytes(stringToConvert);

            //get the hashed values created by our MD5CryptoServiceProvider
            var hashedByteArray = md5Provider.ComputeHash(byteArray);

            //create a StringBuilder object
            var stringBuilder = new StringBuilder();

            //loop to each each byte
            foreach (var b in hashedByteArray)
            {
                //append it to our StringBuilder
                stringBuilder.Append(b.ToString("x2").ToLower());
            }

            //return the hashed value
            return stringBuilder.ToString();
        }


        /// <summary>
        /// Decodes a string that was encoded with UrlTokenEncode
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static byte[] UrlTokenDecode(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            int length = input.Length;
            if (length < 1)
            {
                return new byte[0];
            }
            int num2 = input[length - 1] - '0';
            if ((num2 < 0) || (num2 > 10))
            {
                return null;
            }
            char[] inArray = new char[(length - 1) + num2];
            for (int i = 0; i < (length - 1); i++)
            {
                char ch = input[i];
                switch (ch)
                {
                    case '-':
                        inArray[i] = '+';
                        break;

                    case '_':
                        inArray[i] = '/';
                        break;

                    default:
                        inArray[i] = ch;
                        break;
                }
            }
            for (int j = length - 1; j < inArray.Length; j++)
            {
                inArray[j] = '=';
            }
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
            {
                throw new ArgumentNullException("input");
            }
            if (input.Length < 1)
            {
                return String.Empty;
            }
            string str = null;
            int index = 0;
            char[] chArray = null;
            str = Convert.ToBase64String(input);
            if (str == null)
            {
                return null;
            }
            index = str.Length;
            while (index > 0)
            {
                if (str[index - 1] != '=')
                {
                    break;
                }
                index--;
            }
            chArray = new char[index + 1];
            chArray[index] = (char)((0x30 + str.Length) - index);
            for (int i = 0; i < index; i++)
            {
                char ch = str[i];
                switch (ch)
                {
                    case '+':
                        chArray[i] = '-';
                        break;

                    case '/':
                        chArray[i] = '_';
                        break;

                    case '=':
                        chArray[i] = ch;
                        break;

                    default:
                        chArray[i] = ch;
                        break;
                }
            }
            return new string(chArray);
        }

        /// <summary>
        /// Ensures that the folder path endds with a DirectorySeperatorChar
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
        /// Gets the short string helper.
        /// </summary>
        /// <remarks>This is so that unit tests that do not initialize the resolver do not
        /// fail and fall back to defaults. When running the whole Umbraco, CoreBootManager
        /// does initialise the resolver.</remarks>
        private static IShortStringHelper ShortStringHelper
        {
            get
            {
                if (ShortStringHelperResolver.HasCurrent)
                    return ShortStringHelperResolver.Current.Helper;
                if (_helper != null)
                    return _helper;

                // we don't want Umbraco to die because the resolver hasn't been initialized
                // as the ShortStringHelper is too important, so as long as it's not there
                // already, we use a default one. That should never happen, but...
                Logging.LogHelper.Warn<IShortStringHelper>("ShortStringHelperResolver.HasCurrent == false, fallback to default.");
                _helper = new DefaultShortStringHelper().WithDefaultConfig();
                _helper.Freeze();
                return _helper;
            }
        }
        private static IShortStringHelper _helper;

        /// <summary>
        /// Returns a new string in which all occurences of specified strings are replaced by other specified strings.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="replacements">The replacements definition.</param>
        /// <returns>The filtered string.</returns>
        public static string ReplaceMany(this string text, IDictionary<string, string> replacements)
        {
            return ShortStringHelper.ReplaceMany(text, replacements);
        }

        /// <summary>
        /// Returns a new string in which all occurences of specified characters are replaced by a specified character.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="chars">The characters to replace.</param>
        /// <param name="replacement">The replacement character.</param>
        /// <returns>The filtered string.</returns>
        public static string ReplaceMany(this string text, char[] chars, char replacement)
        {
            return ShortStringHelper.ReplaceMany(text, chars, replacement);
        }

        // FORMAT STRINGS

        // note: LegacyShortStringHelper will produce a 100% backward-compatible output for ToUrlAlias.
        // this is the only reason why we keep the method, otherwise it should be removed, and with any other
        // helper we fallback to ToUrlSegment anyway.

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
        [Obsolete("This method should be removed. Use ToUrlSegment instead.")]
        public static string ToUrlAlias(this string value, IDictionary<string, string> charReplacements, bool replaceDoubleDashes, bool stripNonAscii, bool urlEncode)
        {
            var helper = ShortStringHelper;
            var legacy = helper as LegacyShortStringHelper;
            return legacy != null
                ? legacy.LegacyToUrlAlias(value, charReplacements, replaceDoubleDashes, stripNonAscii, urlEncode)
                : helper.CleanStringForUrlSegment(value);
        }

        // note: LegacyShortStringHelper will produce a 100% backward-compatible output for FormatUrl.
        // this is the only reason why we keep the method, otherwise it should be removed, and with any other
        // helper we fallback to ToUrlSegment anyway.

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="url">The text to filter.</param>
        /// <returns>The safe url segment.</returns>
        /// <remarks>
        /// <para>When using the legacy ShortStringHelper, uses <c>UmbracoSettings.UrlReplaceCharacters</c>
        ///  and <c>UmbracoSettings.RemoveDoubleDashesFromUrlReplacing</c>.</para>
        /// <para>Other helpers may use different parameters.</para>
        /// </remarks>
        [Obsolete("This method should be removed. Use ToUrlSegment instead.")]
        public static string FormatUrl(this string url)
        {
            var helper = ShortStringHelper;
            var legacy = helper as LegacyShortStringHelper;
            return legacy != null ? legacy.LegacyFormatUrl(url) : helper.CleanStringForUrlSegment(url);
        }

        // note: LegacyShortStringHelper will produce a 100% backward-compatible output for ToSafeAlias
        // other helpers may not. DefaultShortStringHelper produces better, but non-compatible, results.

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="alias">The text to filter.</param>
        /// <returns>The safe alias.</returns>
        public static string ToSafeAlias(this string alias)
        {
            return ShortStringHelper.CleanStringForSafeAlias(alias);
        }

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="alias">The text to filter.</param>
        /// <param name="camel">A value indicating that we want to camel-case the alias.</param>
        /// <returns>The safe alias.</returns>
        public static string ToSafeAlias(this string alias, bool camel)
        {
            var a = ShortStringHelper.CleanStringForSafeAlias(alias);
            if (string.IsNullOrWhiteSpace(a) || camel == false) return a;
            return char.ToLowerInvariant(a[0]) + a.Substring(1);
        }

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="alias">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe alias.</returns>
        public static string ToSafeAlias(this string alias, CultureInfo culture)
        {
            return ShortStringHelper.CleanStringForSafeAlias(alias, culture);
        }

        /// <summary>
        /// Cleans (but only if required) a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="alias">The text to filter.</param>
        /// <returns>The safe alias.</returns>
        /// <remarks>Checks <c>UmbracoSettings.ForceSafeAliases</c> to determine whether it should filter the text.</remarks>
        public static string ToSafeAliasWithForcingCheck(this string alias)
        {
            return UmbracoConfig.For.UmbracoSettings().Content.ForceSafeAliases ? alias.ToSafeAlias() : alias;
        }

        /// <summary>
        /// Cleans (but only if required) a string, in the context of a specified culture, to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="alias">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe alias.</returns>
        /// <remarks>Checks <c>UmbracoSettings.ForceSafeAliases</c> to determine whether it should filter the text.</remarks>
        public static string ToSafeAliasWithForcingCheck(this string alias, CultureInfo culture)
        {
            return UmbracoConfig.For.UmbracoSettings().Content.ForceSafeAliases ? alias.ToSafeAlias(culture) : alias;
        }

        // note: LegacyShortStringHelper will produce a 100% backward-compatible output for ToUmbracoAlias.
        // this is the only reason why we keep the method, otherwise it should be removed, and with any other
        // helper we fallback to ToSafeAlias anyway.

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="phrase">The text to filter.</param>
        /// <param name="caseType">The case type. THIS PARAMETER IS IGNORED.</param>
        /// <param name="removeSpaces">Indicates whether spaces should be removed. THIS PARAMETER IS IGNORED.</param>
        /// <returns>The safe alias.</returns>
        /// <remarks>CamelCase, and remove spaces, whatever the parameters.</remarks>
        [Obsolete("This method should be removed. Use ToSafeAlias instead.")]
        public static string ToUmbracoAlias(this string phrase, StringAliasCaseType caseType = StringAliasCaseType.CamelCase, bool removeSpaces = false)
        {
            var helper = ShortStringHelper;
            var legacy = helper as LegacyShortStringHelper;
            return legacy != null ? legacy.LegacyCleanStringForUmbracoAlias(phrase) : helper.CleanStringForSafeAlias(phrase);
        }

        // the new methods to get a url segment

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe url segment.</returns>
        public static string ToUrlSegment(this string text)
        {
            return ShortStringHelper.CleanStringForUrlSegment(text);
        }

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe url segment.</returns>
        public static string ToUrlSegment(this string text, CultureInfo culture)
        {
            return ShortStringHelper.CleanStringForUrlSegment(text, culture);
        }

        // note: LegacyShortStringHelper will produce 100% backward-compatible output for ConvertCase.
        // this is the only reason why we keep the method, otherwise it should be removed, and with any other
        // helper we fallback to CleanString(ascii, alias) anyway.

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
        [Obsolete("This method should be removed. Use ToCleanString instead.")]
        public static string ConvertCase(this string phrase, StringAliasCaseType cases)
        {
            var helper = ShortStringHelper;
            var legacy = helper as LegacyShortStringHelper;
            var cases2 = cases.ToCleanStringType() & CleanStringType.CaseMask;
            return legacy != null
                       ? legacy.LegacyConvertStringCase(phrase, cases2)
                       : helper.CleanString(phrase, CleanStringType.Ascii | CleanStringType.ConvertCase | cases2);
        }

        // the new methods to clean a string (to alias, url segment...)

        /// <summary>
        /// Cleans a string.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the IShortStringHelper default culture.</remarks>
        public static string ToCleanString(this string text, CleanStringType stringType)
        {
            return ShortStringHelper.CleanString(text, stringType);
        }

        /// <summary>
        /// Cleans a string, using a specified separator.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the IShortStringHelper default culture.</remarks>
        public static string ToCleanString(this string text, CleanStringType stringType, char separator)
        {
            return ShortStringHelper.CleanString(text, stringType, separator);
        }

        /// <summary>
        /// Cleans a string in the context of a specified culture.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The clean string.</returns>
        public static string ToCleanString(this string text, CleanStringType stringType, CultureInfo culture)
        {
            return ShortStringHelper.CleanString(text, stringType, culture);
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
        public static string ToCleanString(this string text, CleanStringType stringType, char separator, CultureInfo culture)
        {
            return ShortStringHelper.CleanString(text, stringType, separator, culture);
        }

        // note: LegacyShortStringHelper will produce 100% backward-compatible output for SplitPascalCasing.
        // other helpers may not. DefaultShortStringHelper produces better, but non-compatible, results.

        /// <summary>
        /// Splits a Pascal cased string into a phrase seperated by spaces.
        /// </summary>
        /// <param name="phrase">The text to split.</param>
        /// <returns>The splitted text.</returns>
        public static string SplitPascalCasing(this string phrase)
        {
            return ShortStringHelper.SplitPascalCasing(phrase, ' ');
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe filename.</returns>
        public static string ToSafeFileName(this string text)
        {
            return ShortStringHelper.CleanStringForSafeFileName(text);
        }

        /// <summary>
        /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe filename.</returns>
        public static string ToSafeFileName(this string text, CultureInfo culture)
        {
            return ShortStringHelper.CleanStringForSafeFileName(text, culture);
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
            // This initialisation ensures the first check starts at index zero of the source. On successive checks for
            // a match, the source is skipped to immediately after the last replaced occurrence for efficiency
            // and to avoid infinite loops when oldString and newString compare equal. 
            int index = -1 * newString.Length;

            // Determine if there are any matches left in source, starting from just after the result of replacing the last match.
            while ((index = source.IndexOf(oldString, index + newString.Length, stringComparison)) >= 0)
            {
                // Remove the old text.
                source = source.Remove(index, oldString.Length);

                // Add the replacemenet text.
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

        public static bool ContainsAny(this string haystack, IEnumerable<string> needles, StringComparison comparison = StringComparison.CurrentCulture)
        {
            if (haystack == null) throw new ArgumentNullException("haystack");
            if (string.IsNullOrEmpty(haystack) == false || needles.Any())
            {
                return needles.Any(value => haystack.IndexOf(value) >= 0);
            }
            return false;
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

        // From: http://stackoverflow.com/a/961504/5018
        // filters control characters but allows only properly-formed surrogate sequences
        private static readonly Regex InvalidXmlChars =
            new Regex(
                @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
                RegexOptions.Compiled);


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
            return string.IsNullOrEmpty(text) ? text : InvalidXmlChars.Replace(text, "");
        }
    }
}
