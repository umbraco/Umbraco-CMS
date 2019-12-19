namespace Umbraco.Core.Models.PublishedContent
{
    public static class VariationContextAccessorExtensions
    {
        public static void ContextualizeVariation(this IVariationContextAccessor variationContextAccessor, ContentVariation variations, ref string culture, ref string segment)
        {
            if (culture != null && segment != null) return;

            // use context values
            var publishedVariationContext = variationContextAccessor?.VariationContext;
            if (culture == null) culture = variations.VariesByCulture() ? publishedVariationContext?.Culture : "";
            if (segment == null) segment = variations.VariesBySegment() ? publishedVariationContext?.Segment : "";
        }
    }
}
