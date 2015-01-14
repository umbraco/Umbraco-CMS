using System;

namespace Umbraco.Core.Strings.Css
{
    /// <summary>
    /// Defines a character sequence to filter out when reading.
    /// </summary>
    /// <remarks>
    /// If the sequence exists in the read source, it will be read out as if it was never there.
    /// </remarks>
    internal struct ReadFilter
    {
        #region Fields

        public readonly string StartToken;
        public readonly string EndToken;

        #endregion Fields

        #region Init

        public ReadFilter(string start, string end)
        {
            if (String.IsNullOrEmpty(start))
            {
                throw new ArgumentNullException("start");
            }
            if (String.IsNullOrEmpty(end))
            {
                throw new ArgumentNullException("end");
            }

            this.StartToken = start;
            this.EndToken = end;
        }

        #endregion Init
    }
}