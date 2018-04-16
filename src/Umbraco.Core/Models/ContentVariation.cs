using System;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Indicates the values accepted by a property.
    /// </summary>
    [Flags]
    public enum ContentVariation : byte
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Accepts values for the invariant culture and the neutral segment.
        /// </summary>
        InvariantNeutral = 1,

        /// <summary>
        /// Accepts values for a specified culture and the neutral segment.
        /// </summary>
        CultureNeutral = 2,

        /// <summary>
        /// Accepts values for the invariant culture and a specified segment.
        /// </summary>
        InvariantSegment = 4,

        /// <summary>
        /// Accepts values for a specified culture and a specified segment.
        /// </summary>
        CultureSegment = 8
    }
}
