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

    public async Task AlignPropertyVariance(BlockPropertyValue blockPropertyValue, IPropertyType propertyType, string? culture, string? segment)
    {
        if (propertyType.VariesByCulture() != VariesByCulture(blockPropertyValue))
        {
            blockPropertyValue.Culture = propertyType.VariesByCulture()
                ? culture ?? await _languageService.GetDefaultIsoCodeAsync()
                : null;
        }

        if (propertyType.VariesBySegment() != VariesBySegment(blockPropertyValue))
        {
            blockPropertyValue.Segment = propertyType.VariesByCulture()
                ? segment
                : null;
        }
    }

    public async Task AlignPropertyVariance(BlockPropertyValue blockPropertyValue, IPublishedPropertyType propertyType, IPublishedElement owner)
    {
        ContentVariation propertyTypeVariation = owner.ContentType.Variations & propertyType.Variations;
        if (propertyTypeVariation.VariesByCulture() == VariesByCulture(blockPropertyValue))
        {
            return;
        }

        // mismatch in culture variation for published content:
        // - if the property type varies by culture, assign the current (edited) culture (fallback to the default culture)
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
