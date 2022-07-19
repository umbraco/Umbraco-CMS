// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for content variations.
/// </summary>
public static class ContentVariationExtensions
{
    /// <summary>
    ///     Determines whether the content type is invariant.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type is invariant.
    /// </returns>
    public static bool VariesByNothing(this ISimpleContentType contentType) => contentType.Variations.VariesByNothing();

    /// <summary>
    ///     Determines whether the content type is invariant.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type is invariant.
    /// </returns>
    public static bool VariesByNothing(this IContentTypeBase contentType) => contentType.Variations.VariesByNothing();

    /// <summary>
    ///     Determines whether the content type is invariant.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type is invariant.
    /// </returns>
    public static bool VariesByNothing(this IPublishedContentType contentType) =>
        contentType.Variations.VariesByNothing();

    /// <summary>
    ///     Determines whether the property type is invariant.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     A value indicating whether the property type is invariant.
    /// </returns>
    public static bool VariesByNothing(this IPropertyType propertyType) => propertyType.Variations.VariesByNothing();

    /// <summary>
    ///     Determines whether the property type is invariant.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     A value indicating whether the property type is invariant.
    /// </returns>
    public static bool VariesByNothing(this IPublishedPropertyType propertyType) =>
        propertyType.Variations.VariesByNothing();

    /// <summary>
    ///     Determines whether a variation is invariant.
    /// </summary>
    /// <param name="variation">The variation.</param>
    /// <returns>
    ///     A value indicating whether the variation is invariant.
    /// </returns>
    public static bool VariesByNothing(this ContentVariation variation) => variation == ContentVariation.Nothing;

    /// <summary>
    ///     Determines whether the content type varies by culture.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type varies by culture.
    /// </returns>
    public static bool VariesByCulture(this ISimpleContentType contentType) => contentType.Variations.VariesByCulture();

    /// <summary>
    ///     Determines whether the content type varies by culture.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type varies by culture.
    /// </returns>
    public static bool VariesByCulture(this IContentTypeBase contentType) => contentType.Variations.VariesByCulture();

    /// <summary>
    ///     Determines whether the content type varies by culture.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type varies by culture.
    /// </returns>
    public static bool VariesByCulture(this IPublishedContentType contentType) =>
        contentType.Variations.VariesByCulture();

    /// <summary>
    ///     Determines whether the property type varies by culture.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     A value indicating whether the property type varies by culture.
    /// </returns>
    public static bool VariesByCulture(this IPropertyType propertyType) => propertyType.Variations.VariesByCulture();

    /// <summary>
    ///     Determines whether the property type varies by culture.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     A value indicating whether the property type varies by culture.
    /// </returns>
    public static bool VariesByCulture(this IPublishedPropertyType propertyType) =>
        propertyType.Variations.VariesByCulture();

    /// <summary>
    ///     Determines whether a variation varies by culture.
    /// </summary>
    /// <param name="variation">The variation.</param>
    /// <returns>
    ///     A value indicating whether the variation varies by culture.
    /// </returns>
    public static bool VariesByCulture(this ContentVariation variation) => (variation & ContentVariation.Culture) > 0;

    /// <summary>
    ///     Determines whether the content type varies by segment.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type varies by segment.
    /// </returns>
    public static bool VariesBySegment(this ISimpleContentType contentType) => contentType.Variations.VariesBySegment();

    /// <summary>
    ///     Determines whether the content type varies by segment.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type varies by segment.
    /// </returns>
    public static bool VariesBySegment(this IContentTypeBase contentType) => contentType.Variations.VariesBySegment();

    /// <summary>
    ///     Determines whether the content type varies by segment.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type varies by segment.
    /// </returns>
    public static bool VariesBySegment(this IPublishedContentType contentType) =>
        contentType.Variations.VariesBySegment();

    /// <summary>
    ///     Determines whether the property type varies by segment.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     A value indicating whether the property type varies by segment.
    /// </returns>
    public static bool VariesBySegment(this IPropertyType propertyType) => propertyType.Variations.VariesBySegment();

    /// <summary>
    ///     Determines whether the property type varies by segment.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     A value indicating whether the property type varies by segment.
    /// </returns>
    public static bool VariesBySegment(this IPublishedPropertyType propertyType) =>
        propertyType.Variations.VariesBySegment();

    /// <summary>
    ///     Determines whether a variation varies by segment.
    /// </summary>
    /// <param name="variation">The variation.</param>
    /// <returns>
    ///     A value indicating whether the variation varies by segment.
    /// </returns>
    public static bool VariesBySegment(this ContentVariation variation) => (variation & ContentVariation.Segment) > 0;

    /// <summary>
    ///     Determines whether the content type varies by culture and segment.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type varies by culture and segment.
    /// </returns>
    public static bool VariesByCultureAndSegment(this ISimpleContentType contentType) =>
        contentType.Variations.VariesByCultureAndSegment();

    /// <summary>
    ///     Determines whether the content type varies by culture and segment.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type varies by culture and segment.
    /// </returns>
    public static bool VariesByCultureAndSegment(this IContentTypeBase contentType) =>
        contentType.Variations.VariesByCultureAndSegment();

    /// <summary>
    ///     Determines whether the content type varies by culture and segment.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>
    ///     A value indicating whether the content type varies by culture and segment.
    /// </returns>
    public static bool VariesByCultureAndSegment(this IPublishedContentType contentType) =>
        contentType.Variations.VariesByCultureAndSegment();

    /// <summary>
    ///     Determines whether the property type varies by culture and segment.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     A value indicating whether the property type varies by culture and segment.
    /// </returns>
    public static bool VariesByCultureAndSegment(this IPropertyType propertyType) =>
        propertyType.Variations.VariesByCultureAndSegment();

    /// <summary>
    ///     Determines whether the property type varies by culture and segment.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     A value indicating whether the property type varies by culture and segment.
    /// </returns>
    public static bool VariesByCultureAndSegment(this IPublishedPropertyType propertyType) =>
        propertyType.Variations.VariesByCultureAndSegment();

    /// <summary>
    ///     Determines whether a variation varies by culture and segment.
    /// </summary>
    /// <param name="variation">The variation.</param>
    /// <returns>
    ///     A value indicating whether the variation varies by culture and segment.
    /// </returns>
    public static bool VariesByCultureAndSegment(this ContentVariation variation) =>
        (variation & ContentVariation.CultureAndSegment) == ContentVariation.CultureAndSegment;

    /// <summary>
    ///     Sets or removes the content type variation depending on the specified value.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <param name="variation">The variation to set or remove.</param>
    /// <param name="value">If set to <c>true</c> sets the variation; otherwise, removes the variation.</param>
    /// <remarks>
    ///     This method does not support setting the variation to nothing.
    /// </remarks>
    public static void SetVariesBy(this IContentTypeBase contentType, ContentVariation variation, bool value = true) =>
        contentType.Variations = contentType.Variations.SetFlag(variation, value);

    /// <summary>
    ///     Sets or removes the property type variation depending on the specified value.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <param name="variation">The variation to set or remove.</param>
    /// <param name="value">If set to <c>true</c> sets the variation; otherwise, removes the variation.</param>
    /// <remarks>
    ///     This method does not support setting the variation to nothing.
    /// </remarks>
    public static void SetVariesBy(this IPropertyType propertyType, ContentVariation variation, bool value = true) =>
        propertyType.Variations = propertyType.Variations.SetFlag(variation, value);

    /// <summary>
    ///     Returns the variations with the variation set or removed depending on the specified value.
    /// </summary>
    /// <param name="variations">The existing variations.</param>
    /// <param name="variation">The variation to set or remove.</param>
    /// <param name="value">If set to <c>true</c> sets the variation; otherwise, removes the variation.</param>
    /// <returns>
    ///     The variations with the variation set or removed.
    /// </returns>
    /// <remarks>
    ///     This method does not support setting the variation to nothing.
    /// </remarks>
    public static ContentVariation SetFlag(this ContentVariation variations, ContentVariation variation, bool value = true) =>
        value
            ? variations | variation // Set flag using bitwise logical OR
            : variations &
              ~variation; // Remove flag using bitwise logical AND with bitwise complement (reversing the bit)

    /// <summary>
    ///     Validates that a combination of culture and segment is valid for the variation.
    /// </summary>
    /// <param name="variation">The variation.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="exact">A value indicating whether to perform exact validation.</param>
    /// <param name="wildcards">A value indicating whether to support wildcards.</param>
    /// <param name="throwIfInvalid">
    ///     A value indicating whether to throw a <see cref="NotSupportedException" /> when the
    ///     combination is invalid.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the combination is valid; otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     Occurs when the combination is invalid, and <paramref name="throwIfInvalid" />
    ///     is true.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         When validation is exact, the combination must match the variation exactly. For instance, if the variation is
    ///         Culture, then
    ///         a culture is required. When validation is not strict, the combination must be equivalent, or more restrictive:
    ///         if the variation is
    ///         Culture, an invariant combination is ok.
    ///     </para>
    ///     <para>
    ///         Basically, exact is for one content type, or one property type, and !exact is for "all property types" of one
    ///         content type.
    ///     </para>
    ///     <para>Both <paramref name="culture" /> and <paramref name="segment" /> can be "*" to indicate "all of them".</para>
    /// </remarks>
    public static bool ValidateVariation(this ContentVariation variation, string? culture, string? segment, bool exact, bool wildcards, bool throwIfInvalid)
    {
        culture = culture?.NullOrWhiteSpaceAsNull();
        segment = segment?.NullOrWhiteSpaceAsNull();

        // if wildcards are disabled, do not allow "*"
        if (!wildcards && (culture == "*" || segment == "*"))
        {
            if (throwIfInvalid)
            {
                throw new NotSupportedException("Variation wildcards are not supported.");
            }

            return false;
        }

        if (variation.VariesByCulture())
        {
            // varies by culture
            // in exact mode, the culture cannot be null
            if (exact && culture == null)
            {
                if (throwIfInvalid)
                {
                    throw new NotSupportedException("Culture may not be null because culture variation is enabled.");
                }

                return false;
            }
        }
        else
        {
            // does not vary by culture
            // the culture cannot have a value
            // unless wildcards and it's "*"
            if (culture != null && !(wildcards && culture == "*"))
            {
                if (throwIfInvalid)
                {
                    throw new NotSupportedException(
                        $"Culture \"{culture}\" is invalid because culture variation is disabled.");
                }

                return false;
            }
        }

        // if it does not vary by segment
        // the segment cannot have a value
        // segment may always be null, even when the ContentVariation.Segment flag is set for this variation,
        // therefore the exact parameter is not used in segment validation.
        if (!variation.VariesBySegment() && segment != null && !(wildcards && segment == "*"))
        {
            if (throwIfInvalid)
            {
                throw new NotSupportedException(
                    $"Segment \"{segment}\" is invalid because segment variation is disabled.");
            }

            return false;
        }

        return true;
    }
}
