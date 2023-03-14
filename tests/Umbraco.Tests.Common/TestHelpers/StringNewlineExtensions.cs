// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Common.TestHelpers;

public static class StringNewLineExtensions
{
    /// <summary>
    ///     Ensures Lf only everywhere.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The filtered text.</returns>
    public static string Lf(this string text)
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
    public static string CrLf(this string text)
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
    ///     Replaces Cr/Lf by a single space.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The filtered text.</returns>
    public static string NoCrLf(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = text.Replace("\r\n", " "); // remove CRLF
        text = text.Replace("\r", " "); // remove CR
        text = text.Replace("\n", " "); // remove LF
        return text;
    }
}
