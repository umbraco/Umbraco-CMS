namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Converts UTF-8 text to ASCII, handling accented characters and transliteration.
/// </summary>
public interface IUtf8ToAsciiConverter
{
    /// <summary>
    /// Converts text to ASCII, returning a new string.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="fallback">Character to use for unmappable characters. Default '?'.</param>
    /// <returns>The ASCII-converted string.</returns>
    string Convert(string? text, char fallback = '?');

    /// <summary>
    /// Converts text to ASCII, writing to output span.
    /// Zero-allocation for callers who provide buffer.
    /// </summary>
    /// <param name="input">The input text span.</param>
    /// <param name="output">The output buffer. Must be at least input.Length * 4.</param>
    /// <param name="fallback">Character to use for unmappable characters. Default '?'.</param>
    /// <returns>Number of characters written to output.</returns>
    int Convert(ReadOnlySpan<char> input, Span<char> output, char fallback = '?');
}
