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

    public async Task AlignPropertyVarianceAsync(BlockPropertyValue blockPropertyValue, IPropertyType propertyType, string? culture)
    {
        culture ??= await _languageService.GetDefaultIsoCodeAsync();
        if (propertyType.VariesByCulture() != VariesByCulture(blockPropertyValue))
        {
            blockPropertyValue.Culture = propertyType.VariesByCulture()
                ? culture
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

    public async Task<IEnumerable<BlockItemVariation>> AlignExposeVarianceForElementAsync(BlockValue blockValue, IPublishedElement owner, IPublishedElement element)
    {
        BlockItemVariation[] blockVariations = blockValue.Expose.Where(v => v.ContentKey == element.Key).ToArray();
        if (blockVariations.Any() is false)
        {
            return blockVariations;
        }

        // in case of mismatch in culture variation for block value variation:
        // - if the expected variation is by culture, assign the default culture to all block variation
        // - if the expected variation is not by culture, use all in block variation from the default culture as invariant

        ContentVariation exposeVariation = owner.ContentType.Variations & element.ContentType.Variations;
        if (exposeVariation.VariesByCulture() && blockVariations.All(v => v.Culture is null))
        {
            var defaultCulture = await _languageService.GetDefaultIsoCodeAsync();
            return blockVariations.Select(v => new BlockItemVariation(v.ContentKey, defaultCulture, v.Segment));
        }

        if (exposeVariation.VariesByCulture() is false && blockVariations.All(v => v.Culture is not null))
        {
            var defaultCulture = await _languageService.GetDefaultIsoCodeAsync();
            return blockVariations
                .Where(v => v.Culture == defaultCulture)
                .Select(v => new BlockItemVariation(v.ContentKey, null, v.Segment))
                .ToList();
        }

        return blockVariations;
    }

    public void AlignExposeVariance(BlockValue blockValue)
    {
        var contentDataToAlign = new List<BlockItemData>();
        foreach (BlockItemVariation variation in blockValue.Expose)
        {
            BlockItemData? contentData = blockValue.ContentData.FirstOrDefault(cd => cd.Key == variation.ContentKey);
            if (contentData is null)
            {
                continue;
            }

            if((variation.Culture is null && contentData.Values.Any(v => v.Culture is not null))
                   || (variation.Culture is not null && contentData.Values.All(v => v.Culture is null)))
            {
                contentDataToAlign.Add(contentData);
            }
        }

        if (contentDataToAlign.Any() is false)
        {
            return;
        }

        blockValue.Expose.RemoveAll(v => contentDataToAlign.Any(cd => cd.Key == v.ContentKey));
        foreach (BlockItemData contentData in contentDataToAlign)
        {
            var omitNullCulture = contentData.Values.Any(v => v.Culture is not null);
            foreach (BlockPropertyValue value in contentData.Values
                         .Where(v => omitNullCulture is false || v.Culture is not null)
                         .DistinctBy(v => v.Culture + v.Segment))
            {
                blockValue.Expose.Add(new BlockItemVariation(contentData.Key, value.Culture, value.Segment));
            }
        }
    }

    private static bool VariesByCulture(BlockPropertyValue blockPropertyValue)
        => blockPropertyValue.Culture.IsNullOrWhiteSpace() is false;

    private static bool VariesBySegment(BlockPropertyValue blockPropertyValue)
        => blockPropertyValue.Segment.IsNullOrWhiteSpace() is false;
}
