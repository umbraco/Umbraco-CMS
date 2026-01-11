// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

public static partial class StringExtensions
{
    /// <summary>
    ///     The namespace for URLs (from RFC 4122, Appendix C).
    ///     See <a href="http://www.ietf.org/rfc/rfc4122.txt">RFC 4122</a>
    /// </summary>
    internal static readonly Guid UrlNamespace = new("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>
    ///     Convert a path to node ids in the order from right to left (deepest to shallowest).
    /// </summary>
    /// <param name="path">The path string expected as a comma delimited collection of integers.</param>
    /// <returns>An array of integers matching the provided path.</returns>
    public static int[] GetIdsFromPathReversed(this string path)
    {
        ReadOnlySpan<char> pathSpan = path.AsSpan();

        // Using the explicit enumerator (while/MoveNext) over the SpanSplitEnumerator in a foreach loop to avoid any compiler
        // boxing of the ref struct enumerator.
        // This prevents potential InvalidProgramException across compilers/JITs ("Cannot create boxed ByRef-like values.").
        MemoryExtensions.SpanSplitEnumerator<char> pathSegmentsEnumerator = pathSpan.Split(Constants.CharArrays.Comma);

        List<int> nodeIds = [];
        while (pathSegmentsEnumerator.MoveNext())
        {
            Range rangeOfPathSegment = pathSegmentsEnumerator.Current;
            if (int.TryParse(pathSpan[rangeOfPathSegment], NumberStyles.Integer, CultureInfo.InvariantCulture, out int pathSegment))
            {
                nodeIds.Add(pathSegment);
            }
        }

        var result = new int[nodeIds.Count];
        var resultIndex = 0;
        for (int i = nodeIds.Count - 1; i >= 0; i--)
        {
            result[resultIndex++] = nodeIds[i];
        }

        return result;
    }

    /// <summary>
    ///     Returns a stream from a string
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

    /// <summary>enum try parse.</summary>
    /// <param name="strType">The str type.</param>
    /// <param name="ignoreCase">The ignore case.</param>
    /// <param name="result">The result.</param>
    /// <typeparam name="T">The type</typeparam>
    /// <returns>The enum try parse.</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By Design")]
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By Design")]
    public static bool EnumTryParse<T>(this string strType, bool ignoreCase, out T? result)
    {
        try
        {
            result = (T)Enum.Parse(typeof(T), strType, ignoreCase);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    ///     Parse string to Enum
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="strType">The string to parse</param>
    /// <param name="ignoreCase">The ignore case</param>
    /// <returns>The parsed enum</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By Design")]
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By Design")]
    public static T EnumParse<T>(this string strType, bool ignoreCase) => (T)Enum.Parse(typeof(T), strType, ignoreCase);

    /// <summary>
    ///     Tries to parse a string into the supplied type by finding and using the Type's "Parse" method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public static T? ParseInto<T>(this string val) => (T?)val.ParseInto(typeof(T));

    /// <summary>
    ///     Tries to parse a string into the supplied type by finding and using the Type's "Parse" method
    /// </summary>
    /// <param name="val"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object? ParseInto(this string val, Type type)
    {
        if (string.IsNullOrEmpty(val) == false)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(type);
            return tc.ConvertFrom(val);
        }

        return val;
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

    // having benchmarked various solutions (incl. for/foreach, split and LINQ based ones),
    // this is by far the fastest way to find string needles in a string haystack
    public static int CountOccurrences(this string haystack, string needle)
        => haystack.Length - haystack.Replace(needle, string.Empty).Length;
}
