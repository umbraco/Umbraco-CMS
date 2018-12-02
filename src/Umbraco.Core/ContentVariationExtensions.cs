using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods for content variations.
    /// </summary>
    public static class ContentVariationExtensions
    {
        /// <summary>
        /// Determines whether the content type is invariant.
        /// </summary>
        public static bool VariesByNothing(this IContentTypeBase contentType) => contentType.Variations.VariesByNothing();

        /// <summary>
        /// Determines whether the content type varies by culture.
        /// </summary>
        /// <remarks>And then it could also vary by segment.</remarks>
        public static bool VariesByCulture(this IContentTypeBase contentType) => contentType.Variations.VariesByCulture();

        /// <summary>
        /// Determines whether the content type varies by segment.
        /// </summary>
        /// <remarks>And then it could also vary by culture.</remarks>
        public static bool VariesBySegment(this IContentTypeBase contentType) => contentType.Variations.VariesBySegment();

        /// <summary>
        /// Determines whether the content type varies by culture and segment.
        /// </summary>
        public static bool VariesByCultureAndSegment(this IContentTypeBase contentType) => contentType.Variations.VariesByCultureAndSegment();

        /// <summary>
        /// Determines whether the property type is invariant.
        /// </summary>
        public static bool VariesByNothing(this PropertyType propertyType) => propertyType.Variations.VariesByNothing();

        /// <summary>
        /// Determines whether the property type varies by culture.
        /// </summary>
        /// <remarks>And then it could also vary by segment.</remarks>
        public static bool VariesByCulture(this PropertyType propertyType) => propertyType.Variations.VariesByCulture();

        /// <summary>
        /// Determines whether the property type varies by segment.
        /// </summary>
        /// <remarks>And then it could also vary by culture.</remarks>
        public static bool VariesBySegment(this PropertyType propertyType) => propertyType.Variations.VariesBySegment();

        /// <summary>
        /// Determines whether the property type varies by culture and segment.
        /// </summary>
        public static bool VariesByCultureAndSegment(this PropertyType propertyType) => propertyType.Variations.VariesByCultureAndSegment();

        /// <summary>
        /// Determines whether the content type is invariant.
        /// </summary>
        public static bool VariesByNothing(this PublishedContentType contentType) => contentType.Variations.VariesByNothing();

        /// <summary>
        /// Determines whether the content type varies by culture.
        /// </summary>
        /// <remarks>And then it could also vary by segment.</remarks>
        public static bool VariesByCulture(this PublishedContentType contentType) => contentType.Variations.VariesByCulture();

        /// <summary>
        /// Determines whether the content type varies by segment.
        /// </summary>
        /// <remarks>And then it could also vary by culture.</remarks>
        public static bool VariesBySegment(this PublishedContentType contentType) => contentType.Variations.VariesBySegment();

        /// <summary>
        /// Determines whether the content type varies by culture and segment.
        /// </summary>
        public static bool VariesByCultureAndSegment(this PublishedContentType contentType) => contentType.Variations.VariesByCultureAndSegment();

        /// <summary>
        /// Determines whether the property type is invariant.
        /// </summary>
        public static bool VariesByNothing(this PublishedPropertyType propertyType) => propertyType.Variations.VariesByNothing();

        /// <summary>
        /// Determines whether the property type varies by culture.
        /// </summary>
        public static bool VariesByCulture(this PublishedPropertyType propertyType) => propertyType.Variations.VariesByCulture();

        /// <summary>
        /// Determines whether the property type varies by segment.
        /// </summary>
        public static bool VariesBySegment(this PublishedPropertyType propertyType) => propertyType.Variations.VariesBySegment();

        /// <summary>
        /// Determines whether the property type varies by culture and segment.
        /// </summary>
        public static bool VariesByCultureAndSegment(this PublishedPropertyType propertyType) => propertyType.Variations.VariesByCultureAndSegment();

        /// <summary>
        /// Determines whether a variation is invariant.
        /// </summary>
        public static bool VariesByNothing(this ContentVariation variation) => variation == ContentVariation.Nothing;

        /// <summary>
        /// Determines whether a variation varies by culture.
        /// </summary>
        /// <remarks>And then it could also vary by segment.</remarks>
        public static bool VariesByCulture(this ContentVariation variation) => (variation & ContentVariation.Culture) > 0;

        /// <summary>
        /// Determines whether a variation varies by segment.
        /// </summary>
        /// <remarks>And then it could also vary by culture.</remarks>
        public static bool VariesBySegment(this ContentVariation variation) => (variation & ContentVariation.Segment) > 0;

        /// <summary>
        /// Determines whether a variation varies by culture and segment.
        /// </summary>
        public static bool VariesByCultureAndSegment(this ContentVariation variation) => (variation & ContentVariation.CultureAndSegment) == ContentVariation.CultureAndSegment;

        /// <summary>
        /// Validates that a combination of culture and segment is valid for the variation.
        /// </summary>
        /// <param name="variation">The variation.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="segment">The segment.</param>
        /// <param name="exact">A value indicating whether to perform exact validation.</param>
        /// <param name="wildcards">A value indicating whether to support wildcards.</param>
        /// <param name="throwIfInvalid">A value indicating whether to throw a <see cref="NotSupportedException"/> when the combination is invalid.</param>
        /// <returns>True if the combination is valid; otherwise false.</returns>
        /// <remarks>
        /// <para>When validation is exact, the combination must match the variation exactly. For instance, if the variation is Culture, then
        /// a culture is required. When validation is not strict, the combination must be equivalent, or more restrictive: if the variation is
        /// Culture, an invariant combination is ok.</para>
        /// <para>Basically, exact is for one content type, or one property type, and !exact is for "all property types" of one content type.</para>
        /// <para>Both <paramref name="culture"/> and <paramref name="segment"/> can be "*" to indicate "all of them".</para>
        /// </remarks>
        /// <exception cref="NotSupportedException">Occurs when the combination is invalid, and <paramref name="throwIfInvalid"/> is true.</exception>
        public static bool ValidateVariation(this ContentVariation variation, string culture, string segment, bool exact, bool wildcards, bool throwIfInvalid)
        {
            culture = culture.NullOrWhiteSpaceAsNull();
            segment = segment.NullOrWhiteSpaceAsNull();

            bool Validate(bool variesBy, string value)
            {
                if (variesBy)
                {
                    // varies by
                    // in exact mode, the value cannot be null (but it can be a wildcard)
                    // in !wildcards mode, the value cannot be a wildcard (but it can be null)
                    if ((exact && value == null) || (!wildcards && value == "*"))
                        return false;
                }
                else
                {
                    // does not vary by value
                    // the value cannot have a value
                    // unless wildcards and it's "*"
                    if (value != null && (!wildcards || value != "*"))
                        return false;
                }

                return true;
            }

            if (!Validate(variation.VariesByCulture(), culture))
            {
                if (throwIfInvalid)
                    throw new NotSupportedException($"Culture value \"{culture ?? "<null>"}\" is invalid.");
                return false;
            }

            if (!Validate(variation.VariesBySegment(), segment))
            {
                if (throwIfInvalid)
                    throw new NotSupportedException($"Segment value \"{segment ?? "<null>"}\" is invalid.");
                return false;
            }

            return true;
        }
    }
}
