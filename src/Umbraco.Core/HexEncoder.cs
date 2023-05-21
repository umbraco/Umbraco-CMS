using System.Runtime.CompilerServices;

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides methods for encoding byte arrays into hexadecimal strings.
/// </summary>
public static class HexEncoder
{
    // LUT's that provide the hexadecimal representation of each possible byte value.
    private static readonly char[] HexLutBase =
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F',
    };

    // The base LUT arranged in 16x each item order. 0 * 16, 1 * 16, .... F * 16
    private static readonly char[] HexLutHi = Enumerable.Range(0, 256).Select(x => HexLutBase[x / 0x10]).ToArray();

    // The base LUT repeated 16x.
    private static readonly char[] HexLutLo = Enumerable.Range(0, 256).Select(x => HexLutBase[x % 0x10]).ToArray();

    /// <summary>
    ///     Converts a <see cref="T:byte[]" /> to a hexadecimal formatted <see cref="string" /> padded to 2 digits.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <returns>The <see cref="string" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(byte[] bytes)
    {
        var length = bytes.Length;
        var chars = new char[length * 2];

        var index = 0;
        for (var i = 0; i < length; i++)
        {
            var byteIndex = bytes[i];
            chars[index++] = HexLutHi[byteIndex];
            chars[index++] = HexLutLo[byteIndex];
        }

        return new string(chars, 0, chars.Length);
    }

    /// <summary>
    ///     Converts a <see cref="T:byte[]" /> to a hexadecimal formatted <see cref="string" /> padded to 2 digits
    ///     and split into blocks with the given char separator.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="blockSize">The block size.</param>
    /// <param name="blockCount">The block count.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(byte[] bytes, char separator, int blockSize, int blockCount)
    {
        var length = bytes.Length;
        var chars = new char[(length * 2) + blockCount];
        var count = 0;
        var size = 0;
        var index = 0;

        for (var i = 0; i < length; i++)
        {
            var byteIndex = bytes[i];
            chars[index++] = HexLutHi[byteIndex];
            chars[index++] = HexLutLo[byteIndex];

            if (count == blockCount)
            {
                continue;
            }

            if (++size < blockSize)
            {
                continue;
            }

            chars[index++] = separator;
            size = 0;
            count++;
        }

        return new string(chars, 0, chars.Length);
    }
}
