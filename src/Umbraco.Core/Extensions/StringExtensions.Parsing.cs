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
    /// The namespace for URLs (from RFC 4122, Appendix C).
    /// See <a href="http://www.ietf.org/rfc/rfc4122.txt">RFC 4122</a>.
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles (internal Guid is clearer without a _ prefix).
    internal static readonly Guid UrlNamespace = new("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
#pragma warning restore IDE1006 // Naming Styles

    /// <summary>
    /// Extracts the parent ID from a path string.
    /// </summary>
    /// <param name="path">The path string, expected as a comma-delimited collection of integers (e.g., "-1,1234,5678").</param>
    /// <returns>The parent ID (second-to-last segment), or <see cref="Constants.System.Root"/> if the path has no commas.</returns>
    public static int GetParentIdFromPath(this string path)
    {
        ReadOnlySpan<char> pathSpan = path.AsSpan();
        var lastCommaIndex = pathSpan.LastIndexOf(',');

        if (lastCommaIndex <= 0)
        {
            return Constants.System.Root;
        }

        ReadOnlySpan<char> beforeLastSegment = pathSpan[..lastCommaIndex];
        var secondLastCommaIndex = beforeLastSegment.LastIndexOf(',');

        ReadOnlySpan<char> parentIdSpan = secondLastCommaIndex < 0
            ? beforeLastSegment
            : beforeLastSegment[(secondLastCommaIndex + 1)..];

        return int.Parse(parentIdSpan, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a path string to an array of node IDs in reverse order (deepest to shallowest).
    /// </summary>
    /// <param name="path">The path string, expected as a comma-delimited collection of integers.</param>
    /// <returns>An array of integers matching the provided path, in reverse order.</returns>
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
    /// Returns a stream containing the string content.
    /// </summary>
    /// <param name="s">The string to convert to a stream.</param>
    /// <returns>A <see cref="MemoryStream"/> containing the string content.</returns>
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
    /// Attempts to parse a string to an enumeration value.
    /// </summary>
    /// <typeparam name="T">The enumeration type to parse to.</typeparam>
    /// <param name="strType">The string value to parse.</param>
    /// <param name="ignoreCase">A value indicating whether to ignore case when parsing.</param>
    /// <param name="result">When this method returns, contains the parsed enumeration value if successful; otherwise, the default value.</param>
    /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
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
    /// Parses a string to an enumeration value.
    /// </summary>
    /// <typeparam name="T">The enumeration type to parse to.</typeparam>
    /// <param name="strType">The string value to parse.</param>
    /// <param name="ignoreCase">A value indicating whether to ignore case when parsing.</param>
    /// <returns>The parsed enumeration value.</returns>
    /// <exception cref="ArgumentException">Thrown when the string cannot be parsed to the specified enumeration type.</exception>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By Design")]
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By Design")]
    public static T EnumParse<T>(this string strType, bool ignoreCase) => (T)Enum.Parse(typeof(T), strType, ignoreCase);

    /// <summary>
    /// Tries to parse a string into the specified type using the type's registered type converter.
    /// </summary>
    /// <typeparam name="T">The type to parse the string into.</typeparam>
    /// <param name="val">The string value to parse.</param>
    /// <returns>The parsed value, or the original string if it is null or empty.</returns>
    public static T? ParseInto<T>(this string val) => (T?)val.ParseInto(typeof(T));

    /// <summary>
    /// Tries to parse a string into the specified type using the type's registered type converter.
    /// </summary>
    /// <param name="val">The string value to parse.</param>
    /// <param name="type">The type to parse the string into.</param>
    /// <returns>The parsed value, or the original string if it is null or empty.</returns>
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
    /// Converts a string to a deterministic GUID using a name-based UUID algorithm.
    /// </summary>
    /// <param name="text">The text to convert to a GUID.</param>
    /// <returns>A deterministic GUID derived from the text.</returns>
    /// <remarks>
    /// WARNING: Depending on the string, this may not be unique. The same input will always produce the same GUID.
    /// </remarks>
    public static Guid ToGuid(this string text) =>
        CreateGuidFromHash(
            UrlNamespace,
            text,
            CryptoConfig.AllowOnlyFipsAlgorithms ? 5 // SHA1
                : 3); // MD5

    /// <summary>
    /// Creates a name-based UUID using the algorithm from RFC 4122 section 4.3.
    /// </summary>
    /// <param name="namespaceId">The ID of the namespace.</param>
    /// <param name="name">The name within that namespace.</param>
    /// <param name="version">
    /// The version number of the UUID to create; this value must be either
    /// 3 (for MD5 hashing) or 5 (for SHA-1 hashing).
    /// </param>
    /// <returns>A UUID derived from the namespace and name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when version is not 3 or 5.</exception>
    /// <remarks>
    /// See <a href="http://code.logos.com/blog/2011/04/generating_a_deterministic_guid.html">Generating a deterministic GUID</a>
    /// and <a href="https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/GuidUtility.cs#L34">GuidUtility.cs</a>
    /// for the original implementation.
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

        // Convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3).
        // ASSUME: UTF-8 encoding is always appropriate
        var nameBytes = Encoding.UTF8.GetBytes(name);

        // Convert the namespace UUID to network order (step 3).
        var namespaceBytes = namespaceId.ToByteArray();
        SwapByteOrder(namespaceBytes);

        // Compute the hash of the name space ID concatenated with the name (step 4).
        byte[] hash;
        using (HashAlgorithm algorithm = version == 3 ? MD5.Create() : SHA1.Create())
        {
            algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
            algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
            hash = algorithm.Hash!;
        }

        // Most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12).
        Span<byte> newGuid = hash.AsSpan()[..16];

        // Set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8).
        newGuid[6] = (byte)((newGuid[6] & 0x0F) | (version << 4));

        // Set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10).
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

        // Convert the resulting UUID to local byte order (step 13).
        SwapByteOrder(newGuid);
        return new Guid(newGuid);
    }

    /// <summary>
    /// Converts a GUID (expressed as a byte array) to or from network order (MSB-first).
    /// </summary>
    /// <param name="guid">The byte array representation of the GUID to convert.</param>
    internal static void SwapByteOrder(Span<byte> guid)
    {
        SwapBytes(guid, 0, 3);
        SwapBytes(guid, 1, 2);
        SwapBytes(guid, 4, 5);
        SwapBytes(guid, 6, 7);
    }

    /// <summary>
    /// Swaps two bytes at the specified positions in a byte span.
    /// </summary>
    /// <param name="guid">The byte span containing the bytes to swap.</param>
    /// <param name="left">The index of the first byte to swap.</param>
    /// <param name="right">The index of the second byte to swap.</param>
    private static void SwapBytes(Span<byte> guid, int left, int right) => (guid[left], guid[right]) = (guid[right], guid[left]);

    /// <summary>
    /// Counts the number of occurrences of a substring within a string.
    /// </summary>
    /// <param name="haystack">The string to search within.</param>
    /// <param name="needle">The substring to count.</param>
    /// <returns>The number of times the needle appears in the haystack.</returns>
    /// <remarks>
    /// Having benchmarked various solutions (including for/foreach, split and LINQ-based ones),
    /// this is by far the fastest way to find string needles in a string haystack.
    /// </remarks>
    public static int CountOccurrences(this string haystack, string needle)
        => haystack.Length - haystack.Replace(needle, string.Empty).Length;
}
