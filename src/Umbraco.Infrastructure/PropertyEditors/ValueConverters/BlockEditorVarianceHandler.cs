using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public sealed class BlockEditorVarianceHandler
{
    private readonly ILanguageService _languageService;
    private readonly IContentTypeService _contentTypeService;

    [Obsolete("Please use the constructor that accepts IContentTypeService. Will be removed in V16.")]
    public BlockEditorVarianceHandler(ILanguageService languageService)
        : this(languageService, StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>())
    {
    }

    public BlockEditorVarianceHandler(ILanguageService languageService, IContentTypeService contentTypeService)
    {
        _languageService = languageService;
        _contentTypeService = contentTypeService;
    }

    [Obsolete("Please use the method that allows alignment for a collection of values. Scheduled for removal in V17.")]
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

    /// <summary>
    /// Aligns a collection of block property values for variance changes.
    /// </summary>
    /// <param name="blockPropertyValues">The block property values to align.</param>
    /// <param name="culture">The culture being handled (null if invariant).</param>
    /// <remarks>
    /// Used for aligning variance changes when editing content.
    /// </remarks>
    public async Task<IList<BlockPropertyValue>> AlignPropertyVarianceAsync(IList<BlockPropertyValue> blockPropertyValues, string? culture)
    {
        var defaultIsoCodeAsync = await _languageService.GetDefaultIsoCodeAsync();
        culture ??= defaultIsoCodeAsync;

        var valuesToRemove = new List<BlockPropertyValue>();
        foreach (BlockPropertyValue blockPropertyValue in blockPropertyValues)
        {
            IPropertyType? propertyType = blockPropertyValue.PropertyType;
            if (propertyType is null)
            {
                throw new ArgumentException("One or more block properties did not have a resolved property type. Block editor values must be resolved before attempting to map them to editor.", nameof(blockPropertyValues));
            }

            if (propertyType.VariesByCulture() == VariesByCulture(blockPropertyValue))
            {
                continue;
            }

            if (propertyType.VariesByCulture() is false && blockPropertyValue.Culture.InvariantEquals(defaultIsoCodeAsync) is false)
            {
                valuesToRemove.Add(blockPropertyValue);
            }
            else
            {
                blockPropertyValue.Culture = propertyType.VariesByCulture()
                    ? culture
                    : null;
            }
        }

        return blockPropertyValues.Except(valuesToRemove).ToList();
    }

    /// <summary>
    /// Aligns a block property value for variance changes.
    /// </summary>
    /// <param name="blockPropertyValue">The block property value to align.</param>
    /// <param name="propertyType">The underlying property type.</param>
    /// <param name="owner">The containing block element.</param>
    /// <remarks>
    /// Used for aligning variance changes when rendering content.
    /// </remarks>
    public async Task<BlockPropertyValue?> AlignedPropertyVarianceAsync(BlockPropertyValue blockPropertyValue, IPublishedPropertyType propertyType, IPublishedElement owner)
    {
        ContentVariation propertyTypeVariation = owner.ContentType.Variations & propertyType.Variations;
        if (propertyTypeVariation.VariesByCulture() == VariesByCulture(blockPropertyValue))
        {
            return blockPropertyValue;
        }

        // mismatch in culture variation for published content:
        // - if the property type varies by culture, assign the default culture
        // - if the property type does not vary by culture:
        //   - if the property value culture equals the default culture, assign a null value for it to be rendered as the invariant value
        //   - otherwise return null (not applicable for rendering)
        var defaultCulture = await _languageService.GetDefaultIsoCodeAsync();
        if (propertyTypeVariation.VariesByCulture())
        {
            return new BlockPropertyValue
            {
                Alias = blockPropertyValue.Alias,
                Culture = defaultCulture,
                Segment = blockPropertyValue.Segment,
                Value = blockPropertyValue.Value,
                PropertyType = blockPropertyValue.PropertyType
            };
        }

        if (defaultCulture.Equals(blockPropertyValue.Culture))
        {
            return new BlockPropertyValue
            {
                Alias = blockPropertyValue.Alias,
                Culture = null,
                Segment = blockPropertyValue.Segment,
                Value = blockPropertyValue.Value,
                PropertyType = blockPropertyValue.PropertyType
            };
        }

        return null;
    }

    /// <summary>
    /// Aligns a block value for variance changes.
    /// </summary>
    /// <param name="blockValue">The block property value to align.</param>
    /// <param name="owner">The owner element (the content for block properties at content level, or the parent element for nested block properties).</param>
    /// <param name="element">The containing block element.</param>
    /// <remarks>
    /// Used for aligning variance changes when rendering content.
    /// </remarks>
    public async Task<IEnumerable<BlockItemVariation>> AlignedExposeVarianceAsync(BlockValue blockValue, IPublishedElement owner, IPublishedElement element)
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

    /// <summary>
    /// Aligns block value expose for variance changes.
    /// </summary>
    /// <param name="blockValue">The block value to align.</param>
    /// <remarks>
    /// <para>
    /// Used for aligning variance changes when editing content.
    /// </para>
    /// <para>
    /// This is expected to be invoked after all block values have been aligned for variance changes by <see cref="AlignPropertyVarianceAsync"/>.
    /// </para>
    /// </remarks>
    public void AlignExposeVariance(BlockValue blockValue)
    {
        var contentDataToAlign = new List<BlockItemData>();
        var elementTypesByKey = blockValue
            .ContentData
            .Select(cd => cd.ContentTypeKey)
            .Distinct()
            .Select(_contentTypeService.Get)
            .WhereNotNull()
            .ToDictionary(c => c.Key);

        foreach (BlockItemVariation variation in blockValue.Expose)
        {
            BlockItemData? contentData = blockValue.ContentData.FirstOrDefault(cd => cd.Key == variation.ContentKey);
            if (contentData is null)
            {
                continue;
            }

            if (elementTypesByKey.TryGetValue(contentData.ContentTypeKey, out IContentType? elementType) is false)
            {
                continue;
            }

            if (variation.Culture is not null == elementType.VariesByCulture())
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

        blockValue.Expose = blockValue.Expose.DistinctBy(e => $"{e.ContentKey}.{e.Culture}.{e.Segment}").ToList();
    }

    private static bool VariesByCulture(BlockPropertyValue blockPropertyValue)
        => blockPropertyValue.Culture.IsNullOrWhiteSpace() is false;
}
