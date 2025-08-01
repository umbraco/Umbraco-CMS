// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class VariationContextAccessorExtensions
{
    [Obsolete("Please use the method overload that accepts all parameters. Scheduled for removal in Umbraco 18.")]
    public static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        ref string? culture,
        ref string? segment)
        => variationContextAccessor.ContextualizeVariation(variations, null, null, ref culture, ref segment);

    public static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        string? propertyAlias,
        ref string? culture,
        ref string? segment)
        => variationContextAccessor.ContextualizeVariation(variations, null, propertyAlias, ref culture, ref segment);

    [Obsolete("Please use the method overload that accepts all parameters. Scheduled for removal in Umbraco 18.")]
    public static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        int contentId,
        ref string? culture,
        ref string? segment)
        => variationContextAccessor.ContextualizeVariation(variations, (int?)contentId, null, ref culture, ref segment);

    public static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        int contentId,
        string? propertyAlias,
        ref string? culture,
        ref string? segment)
        => variationContextAccessor.ContextualizeVariation(variations, (int?)contentId, propertyAlias, ref culture, ref segment);

    private static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        int? contentId,
        string? propertyAlias,
        ref string? culture,
        ref string? segment)
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
                        publishedVariationContext?.GetSegment(contentId.Value, propertyAlias);
                }
            }
            else
            {
                segment = string.Empty;
            }
        }
    }
}
