// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Cryptography;
using System.Text;

namespace Umbraco.Extensions;

/// <summary>
/// Encoding, hashing, and serialization extensions.
/// </summary>
public static partial class StringExtensions
{
    private static readonly char[] ToCSharpHexDigitLower = "0123456789abcdef".ToCharArray();
    private static readonly char[] ToCSharpEscapeChars;

    /// <summary>
    ///     The namespace for URLs (from RFC 4122, Appendix C).
    ///     See <a href="http://www.ietf.org/rfc/rfc4122.txt">RFC 4122</a>
    /// </summary>
    internal static readonly Guid UrlNamespace = new("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    static StringExtensions()
    {
        var escapes = new[] { "\aa", "\bb", "\ff", "\nn", "\rr", "\tt", "\vv", "\"\"", "\\\\", "??", "\00" };
        ToCSharpEscapeChars = new char[escapes.Max(e => e[0]) + 1];
        foreach (var escape in escapes)
        {
            ToCSharpEscapeChars[escape[0]] = escape[1];
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
    ///     Converts a string to a Guid - WARNING, depending on the string, this may not be unique
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Guid ToGuid(this string text) =>
        CreateGuidFromHash(
            UrlNamespace,
            text,
            CryptoConfig.AllowOnlyFipsAlgorithms ? 5 // SHA1
                : 3); // MD5

    /// <summary>
    ///     Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
    ///     See
    ///     <a href="https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/GuidUtility.cs#L34">GuidUtility.cs</a>
    ///     for original implementation.
    /// </summary>
    /// <param name="namespaceId">The ID of the namespace.</param>
    /// <param name="name">The name (within that namespace).</param>
    /// <param name="version">
    ///     The version number of the UUID to create; this value must be either
    ///     3 (for MD5 hashing) or 5 (for SHA-1 hashing).
    /// </param>
    /// <returns>A UUID derived from the namespace and name.</returns>
    /// <remarks>
    ///     See
    ///     <a href="http://code.logos.com/blog/2011/04/generating_a_deterministic_guid.html">Generating a deterministic GUID</a>
    ///     .
    /// </remarks>
    internal static Guid CreateGuidFromHash(Guid namespaceId, string name, int version)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        if (version != 3 && version != 5)
        {
            throw new ArgumentOutOfRangeException("version", "version must be either 3 or 5.");
        }

        // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
        // ASSUME: UTF-8 encoding is always appropriate
        var nameBytes = Encoding.UTF8.GetBytes(name);

        // convert the namespace UUID to network order (step 3)
        var namespaceBytes = namespaceId.ToByteArray();
        SwapByteOrder(namespaceBytes);

        // comput the hash of the name space ID concatenated with the name (step 4)
        byte[] hash;
        using (HashAlgorithm algorithm = version == 3 ? MD5.Create() : SHA1.Create())
        {
            algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
            algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
            hash = algorithm.Hash!;
        }

        // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
        Span<byte> newGuid = hash.AsSpan()[..16];

        // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
        newGuid[6] = (byte)((newGuid[6] & 0x0F) | (version << 4));

        // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

        // convert the resulting UUID to local byte order (step 13)
        SwapByteOrder(newGuid);
        return new Guid(newGuid);
    }

    // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
    internal static void SwapByteOrder(Span<byte> guid)
    {
        SwapBytes(guid, 0, 3);
        SwapBytes(guid, 1, 2);
        SwapBytes(guid, 4, 5);
        SwapBytes(guid, 6, 7);
    }

    private static void SwapBytes(Span<byte> guid, int left, int right) => (guid[left], guid[right]) = (guid[right], guid[left]);

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
        HashAlgorithm? hasher = null;

        // create an instance of the correct hashing provider based on the type passed in
        if (hashType is not null)
        {
            hasher = HashAlgorithm.Create(hashType);
        }

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
}
