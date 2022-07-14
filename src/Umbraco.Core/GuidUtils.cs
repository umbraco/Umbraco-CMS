using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Umbraco.Cms.Core;

/// <summary>
///     Utility methods for the <see cref="Guid" /> struct.
/// </summary>
public static class GuidUtils
{
    private static readonly char[] Base32Table =
    {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
        'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5',
    };

    /// <summary>
    ///     Combines two guid instances utilizing an exclusive disjunction.
    ///     The resultant guid is not guaranteed to be unique since the number of unique bits is halved.
    /// </summary>
    /// <param name="a">The first guid.</param>
    /// <param name="b">The seconds guid.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid Combine(Guid a, Guid b)
    {
        var ad = new DecomposedGuid(a);
        var bd = new DecomposedGuid(b);

        ad.Hi ^= bd.Hi;
        ad.Lo ^= bd.Lo;

        return ad.Value;
    }

    /// <summary>
    ///     Converts a Guid into a base-32 string.
    /// </summary>
    /// <param name="guid">A Guid.</param>
    /// <param name="length">The string length.</param>
    /// <returns>A base-32 encoded string.</returns>
    /// <remarks>
    ///     <para>
    ///         A base-32 string representation of a Guid is the shortest, efficient, representation
    ///         that is case insensitive (base-64 is case sensitive).
    ///     </para>
    ///     <para>Length must be 1-26, anything else becomes 26.</para>
    /// </remarks>
    public static string ToBase32String(Guid guid, int length = 26)
    {
        if (length <= 0 || length > 26)
        {
            length = 26;
        }

        var bytes = guid.ToByteArray(); // a Guid is 128 bits ie 16 bytes

        // this could be optimized by making it unsafe,
        // and fixing the table + bytes + chars (see Convert.ToBase64CharArray)

        // each block of 5 bytes = 5*8 = 40 bits
        // becomes 40 bits = 8*5 = 8 byte-32 chars
        // a Guid is 3 blocks + 8 bits

        // so it turns into a 3*8+2 = 26 chars string
        var chars = new char[length];

        var i = 0;
        var j = 0;

        while (i < 15)
        {
            if (j == length)
            {
                break;
            }

            chars[j++] = Base32Table[(bytes[i] & 0b1111_1000) >> 3];
            if (j == length)
            {
                break;
            }

            chars[j++] = Base32Table[((bytes[i] & 0b0000_0111) << 2) | ((bytes[i + 1] & 0b1100_0000) >> 6)];
            if (j == length)
            {
                break;
            }

            chars[j++] = Base32Table[(bytes[i + 1] & 0b0011_1110) >> 1];
            if (j == length)
            {
                break;
            }

            chars[j++] = Base32Table[(bytes[i + 1] & 0b0000_0001) | ((bytes[i + 2] & 0b1111_0000) >> 4)];
            if (j == length)
            {
                break;
            }

            chars[j++] = Base32Table[((bytes[i + 2] & 0b0000_1111) << 1) | ((bytes[i + 3] & 0b1000_0000) >> 7)];
            if (j == length)
            {
                break;
            }

            chars[j++] = Base32Table[(bytes[i + 3] & 0b0111_1100) >> 2];
            if (j == length)
            {
                break;
            }

            chars[j++] = Base32Table[((bytes[i + 3] & 0b0000_0011) << 3) | ((bytes[i + 4] & 0b1110_0000) >> 5)];
            if (j == length)
            {
                break;
            }

            chars[j++] = Base32Table[bytes[i + 4] & 0b0001_1111];

            i += 5;
        }

        if (j < length)
        {
            chars[j++] = Base32Table[(bytes[i] & 0b1111_1000) >> 3];
        }

        if (j < length)
        {
            chars[j] = Base32Table[(bytes[i] & 0b0000_0111) << 2];
        }

        return new string(chars);
    }

    /// <summary>
    ///     A decomposed guid. Allows access to the high and low bits without unsafe code.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    private struct DecomposedGuid
    {
        [FieldOffset(00)]
        public readonly Guid Value;
        [FieldOffset(00)]
        public long Hi;
        [FieldOffset(08)]
        public long Lo;

        public DecomposedGuid(Guid value)
            : this() => Value = value;
    }
}
