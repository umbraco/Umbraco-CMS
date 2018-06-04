using Umbraco.Core.Models;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods for various enumerations.
    /// </summary>
    public static class ContentVariationExtensions
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

        /// <summary>
        /// Determines whether a variation does not support culture variations
        /// </summary>
        /// <param name="variation"></param>
        /// <returns></returns>
        public static bool DoesNotSupportCulture(this ContentVariation variation)
        {
            return !variation.HasAny(ContentVariation.CultureNeutral | ContentVariation.CultureSegment);
        }

        /// <summary>
        /// Determines whether a variation does support culture variations
        /// </summary>
        /// <param name="variation"></param>
        /// <returns></returns>
        public static bool DoesSupportCulture(this ContentVariation variation)
        {
            return variation.HasAny(ContentVariation.CultureNeutral | ContentVariation.CultureSegment);
        }

        /// <summary>
        /// Determines whether a variation does not support invariant variations
        /// </summary>
        /// <param name="variation"></param>
        /// <returns></returns>
        public static bool DoesNotSupportInvariant(this ContentVariation variation)
        {
            return !variation.HasAny(ContentVariation.InvariantNeutral | ContentVariation.InvariantSegment);
        }

        /// <summary>
        /// Determines whether a variation does support invariant variations
        /// </summary>
        /// <param name="variation"></param>
        /// <returns></returns>
        public static bool DoesSupportInvariant(this ContentVariation variation)
        {
            return variation.HasAny(ContentVariation.InvariantNeutral | ContentVariation.InvariantSegment);
        }

        /// <summary>
        /// Determines whether a variation does not support segment variations
        /// </summary>
        /// <param name="variation"></param>
        /// <returns></returns>
        public static bool DoesNotSupportSegment(this ContentVariation variation)
        {
            return !variation.HasAny(ContentVariation.InvariantSegment | ContentVariation.CultureSegment);
        }

        /// <summary>
        /// Determines whether a variation does not support neutral variations
        /// </summary>
        /// <param name="variation"></param>
        /// <returns></returns>
        public static bool DoesNotSupportNeutral(this ContentVariation variation)
        {
            return !variation.HasAny(ContentVariation.InvariantNeutral | ContentVariation.CultureNeutral);
        }
    }
}
