namespace Umbraco.Tests
{
    static class StringNewLineExtensions
    {
        /// <summary>
        /// Ensures Lf only everywhere.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The filtered text.</returns>
        public static string Lf(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = text.Replace("\r", ""); // remove CR
            return text;
        }

        /// <summary>
        /// Ensures CrLf everywhere.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The filtered text.</returns>
        public static string CrLf(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = text.Replace("\r", ""); // remove CR
            text = text.Replace("\n", "\r\n"); // add CR everywhere
            return text;
        }
    }
}
