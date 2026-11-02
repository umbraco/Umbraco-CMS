using System.Buffers;

namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Optimized character sets for searching strings with, shared to avoid rebuilding them per call.
    /// </summary>
    public static class CharSearchValues
    {
        /// <summary>
        ///     Search values containing all the invalid file name characters for the current platform.
        /// </summary>
        public static readonly SearchValues<char> InvalidFileNameChars = SearchValues.Create(Path.GetInvalidFileNameChars());
    }
}
