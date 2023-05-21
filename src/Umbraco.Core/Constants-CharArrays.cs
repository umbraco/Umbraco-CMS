namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Char Arrays to avoid allocations
    /// </summary>
    public static class CharArrays
    {
        /// <summary>
        ///     Char array containing only /
        /// </summary>
        public static readonly char[] ForwardSlash = { '/' };

        /// <summary>
        ///     Char array containing only \
        /// </summary>
        public static readonly char[] Backslash = { '\\' };

        /// <summary>
        ///     Char array containing only '
        /// </summary>
        public static readonly char[] SingleQuote = { '\'' };

        /// <summary>
        ///     Char array containing only "
        /// </summary>
        public static readonly char[] DoubleQuote = { '\"' };

        /// <summary>
        ///     Char array containing ' "
        /// </summary>
        public static readonly char[] DoubleQuoteSingleQuote = { '\"', '\'' };

        /// <summary>
        ///     Char array containing only _
        /// </summary>
        public static readonly char[] Underscore = { '_' };

        /// <summary>
        ///     Char array containing \n \r
        /// </summary>
        public static readonly char[] LineFeedCarriageReturn = { '\n', '\r' };

        /// <summary>
        ///     Char array containing \n
        /// </summary>
        public static readonly char[] LineFeed = { '\n' };

        /// <summary>
        ///     Char array containing only ,
        /// </summary>
        public static readonly char[] Comma = { ',' };

        /// <summary>
        ///     Char array containing only &
        /// </summary>
        public static readonly char[] Ampersand = { '&' };

        /// <summary>
        ///     Char array containing only \0
        /// </summary>
        public static readonly char[] NullTerminator = { '\0' };

        /// <summary>
        ///     Char array containing only .
        /// </summary>
        public static readonly char[] Period = { '.' };

        /// <summary>
        ///     Char array containing only ~
        /// </summary>
        public static readonly char[] Tilde = { '~' };

        /// <summary>
        ///     Char array containing ~ /
        /// </summary>
        public static readonly char[] TildeForwardSlash = { '~', '/' };

        /// <summary>
        ///     Char array containing ~ / \
        /// </summary>
        public static readonly char[] TildeForwardSlashBackSlash = { '~', '/', '\\' };

        /// <summary>
        ///     Char array containing only ?
        /// </summary>
        public static readonly char[] QuestionMark = { '?' };

        /// <summary>
        ///     Char array containing ? &
        /// </summary>
        public static readonly char[] QuestionMarkAmpersand = { '?', '&' };

        /// <summary>
        ///     Char array containing XML 1.1 whitespace chars
        /// </summary>
        public static readonly char[] XmlWhitespaceChars = { ' ', '\t', '\r', '\n' };

        /// <summary>
        ///     Char array containing only the Space char
        /// </summary>
        public static readonly char[] Space = { ' ' };

        /// <summary>
        ///     Char array containing only ;
        /// </summary>
        public static readonly char[] Semicolon = { ';' };

        /// <summary>
        ///     Char array containing a comma and a space
        /// </summary>
        public static readonly char[] CommaSpace = { ',', ' ' };

        /// <summary>
        ///     Char array containing  _ -
        /// </summary>
        public static readonly char[] UnderscoreDash = { '_', '-' };

        /// <summary>
        ///     Char array containing =
        /// </summary>
        public static readonly char[] EqualsChar = { '=' };

        /// <summary>
        ///     Char array containing >
        /// </summary>
        public static readonly char[] GreaterThan = { '>' };

        /// <summary>
        ///     Char array containing |
        /// </summary>
        public static readonly char[] VerticalTab = { '|' };
    }
}
