using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public sealed class BlockEditorVarianceHandler
{
    private readonly ILanguageService _languageService;

    public BlockEditorVarianceHandler(ILanguageService languageService)
        => _languageService = languageService;

    public async Task AlignPropertyVarianceAsync(BlockPropertyValue blockPropertyValue, IPropertyType propertyType, string? culture, string? segment)
    {
        culture ??= await _languageService.GetDefaultIsoCodeAsync();
        if (propertyType.VariesByCulture() != VariesByCulture(blockPropertyValue))
        {
            blockPropertyValue.Culture = propertyType.VariesByCulture()
                ? culture
                : null;
        }

        if (propertyType.VariesBySegment() != VariesBySegment(blockPropertyValue))
        {
            blockPropertyValue.Segment = propertyType.VariesBySegment()
                ? segment
                : null;
        }
    }

    public async Task AlignPropertyVarianceAsync(BlockPropertyValue blockPropertyValue, IPublishedPropertyType propertyType, IPublishedElement owner)
    {
        ContentVariation propertyTypeVariation = owner.ContentType.Variations & propertyType.Variations;
        if (propertyTypeVariation.VariesByCulture() == VariesByCulture(blockPropertyValue))
        {
            return;
        }

        // mismatch in culture variation for published content:
        // - if the property type varies by culture, assign the default culture
        // - if the property type does not vary by culture:
        //   - if the property value culture equals the default culture, assign a null value for it to be rendered as the invariant value
        //   - otherwise leave the value culture as-is - it will be skipped by the rendering
        var defaultCulture = await _languageService.GetDefaultIsoCodeAsync();
        blockPropertyValue.Culture = propertyTypeVariation.VariesByCulture()
            ? defaultCulture
            : defaultCulture.Equals(blockPropertyValue.Culture)
                ? null
                : blockPropertyValue.Culture;
    }

    private static bool VariesByCulture(BlockPropertyValue blockPropertyValue)
        => blockPropertyValue.Culture.IsNullOrWhiteSpace() is false;

    private static bool VariesBySegment(BlockPropertyValue blockPropertyValue)
        => blockPropertyValue.Segment.IsNullOrWhiteSpace() is false;
}
