using System.Buffers;
using System.Collections.Frozen;
using System.Globalization;
using System.Text;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// SIMD-optimized UTF-8 to ASCII converter with extensible character mappings.
/// </summary>
/// <remarks>
/// <para>
/// This converter uses a multi-step fallback strategy:
/// 1. Dictionary lookup for special cases (ligatures, Cyrillic, special Latin)
/// 2. Unicode normalization (FormD) for accented Latin characters
/// 3. Control character stripping
/// 4. Whitespace normalization
/// 5. Fallback character for unmapped characters
/// </para>
/// <para>
/// Most accented Latin characters (À, é, ñ, etc.) are handled automatically via
/// Unicode normalization. Dictionary mappings are only needed for characters that
/// don't decompose correctly (ligatures like Æ→AE, Cyrillic, special Latin like Ø→O).
/// </para>
/// </remarks>
public sealed class Utf8ToAsciiConverter : IUtf8ToAsciiConverter
{
    /// <summary>
    /// Maximum expansion ratio for output buffer sizing.
    /// Worst case: single char becomes 4 chars (e.g., Щ→Shch in standard transliteration).
    /// </summary>
    private const int MaxExpansionRatio = 4;

    // SIMD-optimized ASCII detection (uses AVX-512 when available)
    private static readonly SearchValues<char> AsciiPrintable =
        SearchValues.Create(" !\"#$%&'()*+,-./0123456789:;<=>?@" +
                           "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`" +
                           "abcdefghijklmnopqrstuvwxyz{|}~");

    private readonly FrozenDictionary<char, string> _mappings;

    public Utf8ToAsciiConverter(ICharacterMappingLoader mappingLoader)
    {
        _mappings = mappingLoader.LoadMappings();
    }

    /// <inheritdoc />
    public string Convert(string? text, char fallback = '?')
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var input = text.AsSpan();

        // Fast path: all ASCII - no conversion needed
        if (input.IndexOfAnyExcept(AsciiPrintable) == -1)
        {
            return text;
        }

        // Allocate output buffer for worst-case expansion
        var maxLen = text.Length * MaxExpansionRatio;
        char[] arrayBuffer = ArrayPool<char>.Shared.Rent(maxLen);
        try
        {
            var written = Convert(input, arrayBuffer.AsSpan(), fallback);
            return new string(arrayBuffer, 0, written);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arrayBuffer);
        }
    }

    /// <inheritdoc />
    public int Convert(ReadOnlySpan<char> input, Span<char> output, char fallback = '?')
    {
        if (input.IsEmpty)
        {
            return 0;
        }

        var opos = 0;
        var ipos = 0;

        while (ipos < input.Length)
        {
            // Find next non-ASCII character using SIMD
            var remaining = input[ipos..];
            var asciiLen = remaining.IndexOfAnyExcept(AsciiPrintable);

            if (asciiLen == -1)
            {
                // Rest is all ASCII - bulk copy
                remaining.CopyTo(output[opos..]);
                return opos + remaining.Length;
            }

            if (asciiLen > 0)
            {
                // Copy ASCII prefix
                remaining[..asciiLen].CopyTo(output[opos..]);
                opos += asciiLen;
                ipos += asciiLen;
            }

            // Process non-ASCII character
            var c = input[ipos];

            // Handle surrogate pairs (emoji, etc.)
            if (char.IsSurrogate(c))
            {
                output[opos++] = fallback;
                ipos++;
                if (ipos < input.Length && char.IsLowSurrogate(input[ipos]))
                {
                    ipos++; // Skip low surrogate
                }
                continue;
            }

            opos += ProcessNonAscii(c, output[opos..], fallback);
            ipos++;
        }

        return opos;
    }

    private int ProcessNonAscii(char c, Span<char> output, char fallback)
    {
        // 1. Check special cases dictionary (ligatures, Cyrillic, etc.)
        if (_mappings.TryGetValue(c, out var mapped))
        {
            if (mapped.Length == 0)
            {
                return 0; // Empty mapping = strip character
            }
            mapped.AsSpan().CopyTo(output);
            return mapped.Length;
        }

        // 2. Try Unicode normalization (handles most accented chars)
        var normLen = TryNormalize(c, output);
        if (normLen > 0)
        {
            return normLen;
        }

        // 3. Control character handling
        if (char.IsControl(c))
        {
            return 0; // Strip control characters
        }

        // 4. Whitespace normalization
        if (char.IsWhiteSpace(c))
        {
            output[0] = ' ';
            return 1;
        }

        // 5. Fallback for unmapped characters
        output[0] = fallback;
        return 1;
    }

    private static int TryNormalize(char c, Span<char> output)
    {
        // Skip characters that won't normalize to ASCII
        if (c < '\u00C0')
        {
            return 0;
        }

        // Normalize to FormD (decomposed form)
        ReadOnlySpan<char> input = stackalloc char[] { c };
        var normalized = input.ToString().Normalize(NormalizationForm.FormD);

        if (normalized.Length == 0)
        {
            return 0;
        }

        // Copy only base characters (skip combining marks)
        var len = 0;
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);

            // Skip combining marks (diacritics)
            if (category == UnicodeCategory.NonSpacingMark ||
                category == UnicodeCategory.SpacingCombiningMark ||
                category == UnicodeCategory.EnclosingMark)
            {
                continue;
            }

            // Only keep if it's now ASCII
            if (ch < '\u0080')
            {
                output[len++] = ch;
            }
        }

        return len;
    }
}
