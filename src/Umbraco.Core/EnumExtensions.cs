using Umbraco.Core.Models;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods for various enumerations.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Determines whether a variation has all flags set.
        /// </summary>
        public static bool Has(this ContentVariation variation, ContentVariation values)
            => (variation & values) == values;

        /// <summary>
        /// Determines whether a variation has at least a flag set.
        /// </summary>
        public static bool HasAny(this ContentVariation variation, ContentVariation values)
            => (variation & values) != ContentVariation.Unknown;
    }
}
