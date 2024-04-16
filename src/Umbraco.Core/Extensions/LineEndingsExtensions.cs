using System.Runtime.InteropServices;

namespace Umbraco.Cms.Core.Extensions;

public static class LineEndingsExtensions
{
    /// <summary>
    ///     Ensures Lf only everywhere.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The filtered text.</returns>
    private static string Lf(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = text.Replace("\r", string.Empty); // remove CR
        return text;
    }

    /// <summary>
    ///     Ensures CrLf everywhere.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The filtered text.</returns>
    private static string CrLf(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = text.Replace("\r", string.Empty); // remove CR
        text = text.Replace("\n", "\r\n"); // add CRLF everywhere
        return text;
    }

    /// <summary>
    /// Ensures native line endings.
    /// </summary>
    /// <param name="text">the text to ensure native line endings for.</param>
    /// <returns>the text with native line endings.</returns>
    public static string EnsureNativeLineEndings(this string text)
    {
        var useCrLf = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        return useCrLf ? CrLf(text) : Lf(text);
    }
}
