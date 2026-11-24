// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IVariationContextAccessor"/>.
/// </summary>
public static class VariationContextAccessorExtensions
{
    private static readonly Func<IEnumerable<string>?> _noAvailableSegments = () => null;

    /// <summary>
    ///     Contextualizes the variation based on the content variation settings and property alias.
    /// </summary>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="variations">The content variation settings.</param>
    /// <param name="propertyAlias">The property alias.</param>
    /// <param name="culture">The culture, which may be set based on the variation context.</param>
    /// <param name="segment">The segment, which may be set based on the variation context.</param>
    public static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        string? propertyAlias,
        ref string? culture,
        ref string? segment)
        => variationContextAccessor.ContextualizeVariation(variations, null, propertyAlias, ref culture, ref segment, null);

    /// <summary>
    ///     Contextualizes the variation based on the content variation settings, content ID, and property alias.
    /// </summary>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="variations">The content variation settings.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="propertyAlias">The property alias.</param>
    /// <param name="culture">The culture, which may be set based on the variation context.</param>
    /// <param name="segment">The segment, which may be set based on the variation context.</param>
    public static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        int contentId,
        string? propertyAlias,
        ref string? culture,
        ref string? segment)
        => variationContextAccessor.ContextualizeVariation(variations, (int?)contentId, propertyAlias, ref culture, ref segment, null);

    /// <summary>
    ///     Contextualizes the variation based on the content variation settings, content ID, and property alias.
    /// </summary>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="variations">The content variation settings.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="propertyAlias">The property alias.</param>
    /// <param name="culture">The culture, which may be set based on the variation context.</param>
    /// <param name="segment">The segment, which may be set based on the variation context.</param>
    /// <param name="propertySegmentsForCulture">A factory method to resolve the available property value segments for a given culture at source level, or null if none is available. Overloads without this parameter, such as those used when resolving a value through the published value fallback, provide no segment information.</param>
    public static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        int contentId,
        string? propertyAlias,
        ref string? culture,
        ref string? segment,
        Func<string?, IEnumerable<string>?>? propertySegmentsForCulture)
        => variationContextAccessor.ContextualizeVariation(variations, (int?)contentId, propertyAlias, ref culture, ref segment, propertySegmentsForCulture);

    private static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        int? contentId,
        string? propertyAlias,
        ref string? culture,
        ref string? segment,
        Func<string?, IEnumerable<string>?>? propertySegmentsForCulture)
    {
        if (culture != null && segment != null)
        {
            return;
        }

        // use context values
        VariationContext? publishedVariationContext = variationContextAccessor?.VariationContext;
        culture ??= variations.VariesByCulture() ? publishedVariationContext?.Culture : string.Empty;

        if (segment == null)
        {
            if (variations.VariesBySegment())
            {
                if (contentId == null)
                {
                    segment = publishedVariationContext?.Segment;
                }
                else
                {
                    segment = propertyAlias == null ?
                        publishedVariationContext?.GetSegment(contentId.Value) :
                        GetPropertySegment(publishedVariationContext, contentId.Value, propertyAlias, culture, propertySegmentsForCulture);
                }
            }
            else
            {
                segment = string.Empty;
            }
        }
    }

    private static string? GetPropertySegment(
        VariationContext? variationContext,
        int contentId,
        string propertyAlias,
        string? culture,
        Func<string?, IEnumerable<string>?>? propertySegmentsForCulture)
        => variationContext?.GetSegment(
            contentId,
            propertyAlias,
            propertySegmentsForCulture is null ? _noAvailableSegments : BindCulture(propertySegmentsForCulture, culture));

    // Kept separate from GetPropertySegment so the closure is only allocated when there is a factory to bind.
    private static Func<IEnumerable<string>?> BindCulture(Func<string?, IEnumerable<string>?> propertySegmentsForCulture, string? culture)
        => () => propertySegmentsForCulture(culture);
}
