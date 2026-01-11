// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Cryptography;
using System.Text;

namespace Umbraco.Extensions;

public static partial class StringExtensions
{
    private static readonly char[] ToCSharpHexDigitLower = "0123456789abcdef".ToCharArray();
    private static readonly char[] ToCSharpEscapeChars;

    static StringExtensions()
    {
        var escapes = new[] { "\aa", "\bb", "\ff", "\nn", "\rr", "\tt", "\vv", "\"\"", "\\\\", "??", "\00" };
        ToCSharpEscapeChars = new char[escapes.Max(e => e[0]) + 1];
        foreach (var escape in escapes)
        {
            ToCSharpEscapeChars[escape[0]] = escape[1];
        }
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
                    int i = c;
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

    /// <summary>
    ///     Encodes as GUID.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns></returns>
    public static Guid EncodeAsGuid(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentNullException("input");
        }

        var convertToHex = input.ConvertToHex();
        var hexLength = convertToHex.Length < 32 ? convertToHex.Length : 32;
        var hex = convertToHex[..hexLength].PadLeft(32, '0');
        return Guid.TryParse(hex, out Guid output) ? output : Guid.Empty;
    }

    /// <summary>
    ///     Converts to hex.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns></returns>
    public static string ConvertToHex(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            sb.AppendFormat("{0:x2}", Convert.ToUInt32(c));
        }

        return sb.ToString();
    }

    public static string DecodeFromHex(this string hexValue)
    {
        var strValue = string.Empty;
        while (hexValue.Length > 0)
        {
            strValue += Convert.ToChar(Convert.ToUInt32(hexValue[..2], 16)).ToString();
            hexValue = hexValue[2..];
        }

        return strValue;
    }

    /// <summary>
    ///     Encodes a string to a safe URL base64 string
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToUrlBase64(this string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // return Convert.ToBase64String(bytes).Replace(".", "-").Replace("/", "_").Replace("=", ",");
        var bytes = Encoding.UTF8.GetBytes(input);
        return UrlTokenEncode(bytes);
    }

    /// <summary>
    ///     Decodes a URL safe base64 string back
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string? FromUrlBase64(this string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        // if (input.IsInvalidBase64()) return null;
        try
        {
            // var decodedBytes = Convert.FromBase64String(input.Replace("-", ".").Replace("_", "/").Replace(",", "="));
            var decodedBytes = UrlTokenDecode(input);
            return decodedBytes != null ? Encoding.UTF8.GetString(decodedBytes) : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Generates a hash of a string based on the FIPS compliance setting.
    /// </summary>
    /// <param name="str">Refers to itself</param>
    /// <returns>The hashed string</returns>
    public static string GenerateHash(this string str) => str.ToSHA1();

    /// <summary>
    ///     Generate a hash of a string based on the specified hash algorithm.
    /// </summary>
    /// <typeparam name="T">The hash algorithm implementation to use.</typeparam>
    /// <param name="str">The <see cref="string" /> to hash.</param>
    /// <returns>
    ///     The hashed string.
    /// </returns>
    public static string GenerateHash<T>(this string str)
        where T : HashAlgorithm => str.GenerateHash(typeof(T).FullName);

    /// <summary>
    ///     Converts the string to SHA1
    /// </summary>
    /// <param name="stringToConvert">refers to itself</param>
    /// <returns>The SHA1 hashed string</returns>
    public static string ToSHA1(this string stringToConvert) => stringToConvert.GenerateHash("SHA1");

    /// <summary>
    ///     Decodes a string that was encoded with UrlTokenEncode
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static byte[] UrlTokenDecode(this string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (input.Length == 0)
        {
            return [];
        }

        // calc array size - must be groups of 4
        var arrayLength = input.Length;
        var remain = arrayLength % 4;
        if (remain != 0)
        {
            arrayLength += 4 - remain;
        }

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
        {
            inArray[j] = '=';
        }

        return Convert.FromBase64CharArray(inArray, 0, inArray.Length);
    }

    /// <summary>
    ///     Generate a hash of a string based on the hashType passed in
    /// </summary>
    /// <param name="str">Refers to itself</param>
    /// <param name="hashType">
    ///     String with the hash type.  See remarks section of the CryptoConfig Class in MSDN docs for a
    ///     list of possible values.
    /// </param>
    /// <returns>The hashed string</returns>
    private static string GenerateHash(this string str, string? hashType)
    {
        HashAlgorithm? hasher = CreateHashAlgorithm(hashType);

        if (hasher == null)
        {
            throw new InvalidOperationException("No hashing type found by name " + hashType);
        }

        using (hasher)
        {
            // convert our string into byte array
            var byteArray = Encoding.UTF8.GetBytes(str);

            // get the hashed values created by our selected provider
            var hashedByteArray = hasher.ComputeHash(byteArray);

            // create a StringBuilder object
            var stringBuilder = new StringBuilder();

            // loop to each byte
            foreach (var b in hashedByteArray)
            {
                // append it to our StringBuilder
                stringBuilder.Append(b.ToString("x2"));
            }

            // return the hashed value
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    ///     Creates a hash algorithm instance by name.
    /// </summary>
    /// <param name="algorithmName">The algorithm name (e.g., "SHA1", "SHA256", "MD5").</param>
    /// <returns>A HashAlgorithm instance, or null if the algorithm is not recognized.</returns>
    private static HashAlgorithm? CreateHashAlgorithm(string? algorithmName)
    {
        if (string.IsNullOrEmpty(algorithmName))
        {
            return null;
        }

        return algorithmName.ToUpperInvariant() switch
        {
            "SHA1" or "SHA-1" or "SYSTEM.SECURITY.CRYPTOGRAPHY.SHA1" => SHA1.Create(),
            "SHA256" or "SHA-256" or "SYSTEM.SECURITY.CRYPTOGRAPHY.SHA256" => SHA256.Create(),
            "SHA384" or "SHA-384" or "SYSTEM.SECURITY.CRYPTOGRAPHY.SHA384" => SHA384.Create(),
            "SHA512" or "SHA-512" or "SYSTEM.SECURITY.CRYPTOGRAPHY.SHA512" => SHA512.Create(),
            "MD5" or "SYSTEM.SECURITY.CRYPTOGRAPHY.MD5" => MD5.Create(),
            "HMACSHA1" or "SYSTEM.SECURITY.CRYPTOGRAPHY.HMACSHA1" => new HMACSHA1(),
            "HMACSHA256" or "SYSTEM.SECURITY.CRYPTOGRAPHY.HMACSHA256" => new HMACSHA256(),
            "HMACSHA384" or "SYSTEM.SECURITY.CRYPTOGRAPHY.HMACSHA384" => new HMACSHA384(),
            "HMACSHA512" or "SYSTEM.SECURITY.CRYPTOGRAPHY.HMACSHA512" => new HMACSHA512(),
            _ => null
        };
    }

    /// <summary>
    ///     Encodes a string so that it is 'safe' for URLs, files, etc..
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string UrlTokenEncode(this byte[] input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (input.Length == 0)
        {
            return string.Empty;
        }

        // base-64 digits are A-Z, a-z, 0-9, + and /
        // the = char is used for trailing padding
        var str = Convert.ToBase64String(input);

        var pos = str.IndexOf('=');
        if (pos < 0)
        {
            pos = str.Length;
        }

        // replace chars that would cause problems in URLs
        Span<char> chArray = pos <= 1024 ? stackalloc char[pos] : new char[pos];
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
    ///     Converts a literal string into a C# expression.
    /// </summary>
    /// <param name="s">Current instance of the string.</param>
    /// <returns>The string in a C# format.</returns>
    public static string ToCSharpString(this string s)
    {
        if (s == null)
        {
            return "<null>";
        }

        // http://stackoverflow.com/questions/323640/can-i-convert-a-c-sharp-string-value-to-an-escaped-string-literal
        var sb = new StringBuilder(s.Length + 2);
        for (var rp = 0; rp < s.Length; rp++)
        {
            var c = s[rp];
            if (c < ToCSharpEscapeChars.Length && ToCSharpEscapeChars[c] != '\0')
            {
                sb.Append('\\').Append(ToCSharpEscapeChars[c]);
            }
            else if (c <= '~' && c >= ' ')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append(@"\x")
                    .Append(ToCSharpHexDigitLower[(c >> 12) & 0x0F])
                    .Append(ToCSharpHexDigitLower[(c >> 8) & 0x0F])
                    .Append(ToCSharpHexDigitLower[(c >> 4) & 0x0F])
                    .Append(ToCSharpHexDigitLower[c & 0x0F]);
            }
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
}
