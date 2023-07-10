using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core
{
    public static partial class Constants
    {
        /// <summary>
        /// Char Arrays to avoid allocations
        /// </summary>
        public static class CharArrays
        {
            /// <summary>
            /// Char array containing only /
            /// </summary>
            public static readonly char[] ForwardSlash = new char[] { '/' };

            /// <summary>
            /// Char array containing only \
            /// </summary>
            public static readonly char[] Backslash = new char[] { '\\' };

            /// <summary>
            /// Char array containing only '
            /// </summary>
            public static readonly char[] SingleQuote = new char[] { '\'' };

            /// <summary>
            /// Char array containing only "
            /// </summary>
            public static readonly char[] DoubleQuote = new char[] { '\"' };


            /// <summary>
            /// Char array containing ' "
            /// </summary>
            public static readonly char[] DoubleQuoteSingleQuote = new char[] { '\"', '\'' };

            /// <summary>
            /// Char array containing only _
            /// </summary>
            public static readonly char[] Underscore = new char[] { '_' };

            /// <summary>
            /// Char array containing \n \r
            /// </summary>
            public static readonly char[] LineFeedCarriageReturn = new char[] { '\n', '\r' };


            /// <summary>
            /// Char array containing \n
            /// </summary>
            public static readonly char[] LineFeed = new char[] { '\n' };

            /// <summary>
            /// Char array containing only ,
            /// </summary>
            public static readonly char[] Comma = new char[] { ',' };

            /// <summary>
            /// Char array containing only &
            /// </summary>
            public static readonly char[] Ampersand = new char[] { '&' };

            /// <summary>
            /// Char array containing only \0
            /// </summary>
            public static readonly char[] NullTerminator = new char[] { '\0' };

            /// <summary>
            /// Char array containing only .
            /// </summary>
            public static readonly char[] Period = new char[] { '.' };

            /// <summary>
            /// Char array containing only ~
            /// </summary>
            public static readonly char[] Tilde = new char[] { '~' };
            /// <summary>
            /// Char array containing ~ /
            /// </summary>
            public static readonly char[] TildeForwardSlash = new char[] { '~', '/' };

            /// <summary>
            /// Char array containing only ?
            /// </summary>
            public static readonly char[] QuestionMark = new char[] { '?' };

            /// <summary>
            /// Char array containing ? &
            /// </summary>
            public static readonly char[] QuestionMarkAmpersand = new char[] { '?', '&' };

            /// <summary>
            /// Char array containing XML 1.1 whitespace chars
            /// </summary>
            public static readonly char[] XmlWhitespaceChars = new char[] { ' ', '\t', '\r', '\n' };

            /// <summary>
            /// Char array containing only the Space char
            /// </summary>
            public static readonly char[] Space = new char[] { ' ' };

            /// <summary>
            /// Char array containing only ;
            /// </summary>
            public static readonly char[] Semicolon = new char[] { ';' };

            /// <summary>
            /// Char array containing a comma and a space
            /// </summary>
            public static readonly char[] CommaSpace = new char[] { ',', ' ' };

            /// <summary>
            /// Char array containing  _ -
            /// </summary>
            public static readonly char[] UnderscoreDash = new char[] { '_', '-' };

            /// <summary>
            /// Char array containing =
            /// </summary>
            public static readonly char[] EqualsChar = new char[] { '=' };

            /// <summary>
            /// Char array containing >
            /// </summary>
            public static readonly char[] GreaterThan = new char[] { '>' };

            /// <summary>
            /// Char array containing |
            /// </summary>
            public static readonly char[] VerticalTab = new char[] { '|' };
        }
    }
}
