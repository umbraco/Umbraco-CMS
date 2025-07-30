// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class VariationContextAccessorExtensions
{
    public static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        ref string? culture,
        ref string? segment,
        string? propertyAlias)
        => variationContextAccessor.ContextualizeVariation(variations, null, ref culture, ref segment, propertyAlias);

    public static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        int contentId,
        ref string? culture,
        ref string? segment,
        string? propertyAlias)
        => variationContextAccessor.ContextualizeVariation(variations, (int?)contentId, ref culture, ref segment, propertyAlias);

    private static void ContextualizeVariation(
        this IVariationContextAccessor variationContextAccessor,
        ContentVariation variations,
        int? contentId,
        ref string? culture,
        ref string? segment,
        string? propertyAlias)
    {
        if (culture != null && segment != null)
        {
            return;
        }

        // use context values
        VariationContext? publishedVariationContext = variationContextAccessor?.VariationContext;
        if (culture == null)
        {
            culture = variations.VariesByCulture() ? publishedVariationContext?.Culture : string.Empty;
        }

        if (segment == null)
        {
            if (variations.VariesBySegment())
            {
                segment = contentId == null
                    ? publishedVariationContext?.Segment
                    : publishedVariationContext?.GetSegment(contentId.Value, propertyAlias);
            }
            else
            {
                segment = string.Empty;
            }
        }
    }
}
