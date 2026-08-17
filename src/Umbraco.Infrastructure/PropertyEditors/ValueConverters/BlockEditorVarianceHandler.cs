using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Handles value variance for the Block Editor property editor, determining how property values differ based on culture and segment.
/// </summary>
public sealed class BlockEditorVarianceHandler
{
    private readonly ILanguageService _languageService;
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockEditorVarianceHandler"/> class.
    /// </summary>
    /// <param name="languageService">Service used to manage and retrieve language information for localization.</param>
    /// <param name="contentTypeService">Service used to manage and retrieve content type definitions.</param>
    public BlockEditorVarianceHandler(ILanguageService languageService, IContentTypeService contentTypeService)
    {
        _languageService = languageService;
        _contentTypeService = contentTypeService;
    }

    /// <summary>
    /// Aligns a collection of block property values for variance changes.
    /// </summary>
    /// <param name="blockPropertyValues">The block property values to align.</param>
    /// <param name="culture">The culture of the property value being aligned (null if invariant).</param>
    /// <returns>A task that represents the asynchronous operation, containing the aligned block property values.</returns>
    /// <remarks>
    /// <para>Used for aligning variance changes when editing content.</para>
    /// <para>
    /// A property type that has become culture variant adopts <paramref name="culture"/>. A property type that has
    /// become culture invariant can only retain a single value per alias and segment; an explicitly invariant value
    /// takes precedence, then the value for <paramref name="culture"/>, then the value for the default language.
    /// </para>
    /// </remarks>
    public async Task<IList<BlockPropertyValue>> AlignPropertyVarianceAsync(IList<BlockPropertyValue> blockPropertyValues, string? culture)
    {
        var defaultIsoCode = await _languageService.GetDefaultIsoCodeAsync();
        culture ??= defaultIsoCode;

        if (blockPropertyValues.Any(blockPropertyValue => blockPropertyValue.PropertyType is null))
        {
            throw new ArgumentException("One or more block properties did not have a resolved property type. Block editor values must be resolved before attempting to map them to editor.", nameof(blockPropertyValues));
        }

        foreach (BlockPropertyValue blockPropertyValue in blockPropertyValues
                     .Where(blockPropertyValue => blockPropertyValue.PropertyType!.VariesByCulture() && VariesByCulture(blockPropertyValue) is false))
        {
            blockPropertyValue.Culture = culture;
        }

        var valuesToRemove = new HashSet<BlockPropertyValue>();
        IEnumerable<IGrouping<(string Alias, string? Segment), BlockPropertyValue>> collapsingValues = blockPropertyValues
            .Where(blockPropertyValue => blockPropertyValue.PropertyType!.VariesByCulture() is false && VariesByCulture(blockPropertyValue))
            .GroupBy(blockPropertyValue => (blockPropertyValue.Alias, blockPropertyValue.Segment));

        foreach (IGrouping<(string Alias, string? Segment), BlockPropertyValue> group in collapsingValues)
        {
            BlockPropertyValue? valueToRetain = ValueToRetain(blockPropertyValues, group, culture, defaultIsoCode);
            foreach (BlockPropertyValue blockPropertyValue in group)
            {
                if (blockPropertyValue == valueToRetain)
                {
                    blockPropertyValue.Culture = null;
                }
                else
                {
                    valuesToRemove.Add(blockPropertyValue);
                }
            }
        }

        return blockPropertyValues.Where(blockPropertyValue => valuesToRemove.Contains(blockPropertyValue) is false).ToList();
    }

    /// <summary>
    /// Aligns a block property value for variance changes.
    /// </summary>
    /// <param name="blockPropertyValue">The block property value to align.</param>
    /// <param name="propertyType">The underlying property type.</param>
    /// <param name="owner">The containing block element.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the aligned <see cref="BlockPropertyValue"/>, or <c>null</c> if alignment is not applicable.</returns>
    [Obsolete("Please use the overload that takes the culture of the owning property value. Scheduled for removal in Umbraco 19.")]
    public Task<BlockPropertyValue?> AlignedPropertyVarianceAsync(BlockPropertyValue blockPropertyValue, IPublishedPropertyType propertyType, IPublishedElement owner)
        => AlignedPropertyVarianceAsync(blockPropertyValue, propertyType, owner, culture: null);

    /// <summary>
    /// Aligns a block property value for variance changes.
    /// </summary>
    /// <param name="blockPropertyValue">The block property value to align.</param>
    /// <param name="propertyType">The underlying property type.</param>
    /// <param name="owner">The containing block element.</param>
    /// <param name="culture">The culture of the owning property value, or <c>null</c> when the owning property does not vary by culture.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the aligned <see cref="BlockPropertyValue"/>, or <c>null</c> if alignment is not applicable.</returns>
    /// <remarks>
    /// <para>Used for aligning variance changes when rendering content.</para>
    /// <para>
    /// When the owning property varies by culture, each culture is stored separately, so the block property values that
    /// belong to <paramref name="culture"/> are the ones that represent the block. Otherwise all cultures share a single
    /// stored value, and the default language is the one that survives becoming invariant.
    /// </para>
    /// </remarks>
    public async Task<BlockPropertyValue?> AlignedPropertyVarianceAsync(BlockPropertyValue blockPropertyValue, IPublishedPropertyType propertyType, IPublishedElement owner, string? culture)
    {
        ContentVariation propertyTypeVariation = owner.ContentType.Variations & propertyType.Variations;
        if (propertyTypeVariation.VariesByCulture() == VariesByCulture(blockPropertyValue))
        {
            return blockPropertyValue;
        }

        // mismatch in culture variation for published content:
        // - if the property type varies by culture, assign the default culture
        // - if the property type does not vary by culture:
        //   - if the property value culture is the one representing the block, assign a null value for it to be rendered as the invariant value
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

        if ((culture ?? defaultCulture).InvariantEquals(blockPropertyValue.Culture))
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
    /// <param name="blockValue">The block property value to align for variance.</param>
    /// <param name="owner">The owner element, which is either the content for block properties at the content level or the parent element for nested block properties.</param>
    /// <param name="element">The block element containing the property.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing the aligned <see cref="BlockItemVariation"/> instances for the specified block element.</returns>
    [Obsolete("Please use the overload that takes the culture of the owning property value. Scheduled for removal in Umbraco 19.")]
    public Task<IEnumerable<BlockItemVariation>> AlignedExposeVarianceAsync(BlockValue blockValue, IPublishedElement owner, IPublishedElement element)
        => AlignedExposeVarianceAsync(blockValue, owner, element, culture: null);

    /// <summary>
    /// Aligns a block value for variance changes.
    /// </summary>
    /// <param name="blockValue">The block property value to align for variance.</param>
    /// <param name="owner">The owner element, which is either the content for block properties at the content level or the parent element for nested block properties.</param>
    /// <param name="element">The block element containing the property.</param>
    /// <param name="culture">The culture of the owning property value, or <c>null</c> when the owning property does not vary by culture.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing the aligned <see cref="BlockItemVariation"/> instances for the specified block element.</returns>
    /// <remarks>
    /// <para>Used for aligning block item variations according to variance (such as culture or segment) when rendering content.</para>
    /// <para>In case of mismatch in culture variation for block value variation:</para>
    /// <list type="bullet">
    /// <item><description>If the expected variation is by culture but all expose entries are invariant, assign the default culture.</description></item>
    /// <item><description>If the expected variation is invariant but all expose entries have cultures, use the entry for <paramref name="culture"/> as invariant, falling back to the one for the default culture.</description></item>
    /// </list>
    /// </remarks>
    public async Task<IEnumerable<BlockItemVariation>> AlignedExposeVarianceAsync(BlockValue blockValue, IPublishedElement owner, IPublishedElement element, string? culture)
    {
        BlockItemVariation[] blockVariations = blockValue.Expose.Where(v => v.ContentKey == element.Key).ToArray();
        if (blockVariations.Any() is false)
        {
            return blockVariations;
        }

        ContentVariation exposeVariation = owner.ContentType.Variations & element.ContentType.Variations;
        if (exposeVariation.VariesByCulture() && blockVariations.All(v => v.Culture is null))
        {
            var defaultCulture = await _languageService.GetDefaultIsoCodeAsync();
            return blockVariations.Select(v => new BlockItemVariation(v.ContentKey, defaultCulture, v.Segment));
        }

        if (exposeVariation.VariesByCulture() is false && blockVariations.All(v => v.Culture is not null))
        {
            var defaultCulture = await _languageService.GetDefaultIsoCodeAsync();
            BlockItemVariation[] retainedVariations = blockVariations.Where(v => v.Culture.InvariantEquals(culture ?? defaultCulture)).ToArray();
            if (retainedVariations.Length == 0)
            {
                retainedVariations = blockVariations.Where(v => v.Culture.InvariantEquals(defaultCulture)).ToArray();
            }

            return retainedVariations
                .Select(v => new BlockItemVariation(v.ContentKey, null, v.Segment))
                .ToList();
        }

        return blockVariations;
    }

    /// <summary>
    /// Aligns block value expose for variance changes.
    /// </summary>
    /// <param name="blockValue">The block value to align.</param>
    [Obsolete("Please use the overload that takes the culture being aligned. Scheduled for removal in Umbraco 19.")]
    public void AlignExposeVariance(BlockValue blockValue)
        => AlignExposeVariance(blockValue, culture: null);

    /// <summary>
    /// Aligns block value expose for variance changes.
    /// </summary>
    /// <param name="blockValue">The block value to align.</param>
    /// <param name="culture">The culture of the property value being aligned (null if invariant).</param>
    /// <remarks>
    /// <para>
    /// Used for aligning variance changes when editing content.
    /// </para>
    /// <para>
    /// This is expected to be invoked after all block values have been aligned for variance changes by <see cref="AlignPropertyVarianceAsync"/>.
    /// </para>
    /// <para>
    /// A block that holds no property values has no value variance to derive its expose entries from, so it is
    /// exposed for <paramref name="culture"/> when its element type varies by culture, and invariantly when it does not.
    /// </para>
    /// </remarks>
    public void AlignExposeVariance(BlockValue blockValue, string? culture)
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

            if ((variation.Culture is not null) == elementType.VariesByCulture())
            {
                continue;
            }

            if (contentData.Values.Count == 0 ||
                (variation.Culture is null && contentData.Values.Any(v => v.Culture is not null)) ||
                (variation.Culture is not null && contentData.Values.All(v => v.Culture is null)))
            {
                contentDataToAlign.Add(contentData);
            }
        }

        // Remove expose entries that don't have matching entries in the block value's content data.
        var validContentKeys = blockValue.ContentData.Select(cd => cd.Key).ToHashSet();
        blockValue.Expose.RemoveAll(v => validContentKeys.Contains(v.ContentKey) is false);

        if (contentDataToAlign.Count > 0)
        {
            blockValue.Expose.RemoveAll(v => contentDataToAlign.Any(cd => cd.Key == v.ContentKey));
            foreach (BlockItemData contentData in contentDataToAlign)
            {
                var omitNullCulture = contentData.Values.Any(v => v.Culture is not null);
                var alignedVariations = contentData.Values
                    .Where(v => omitNullCulture is false || v.Culture is not null)
                    .DistinctBy(v => v.Culture + v.Segment)
                    .Select(v => new BlockItemVariation(contentData.Key, v.Culture, v.Segment))
                    .ToList();

                if (alignedVariations.Count == 0)
                {
                    alignedVariations.Add(new BlockItemVariation(
                        contentData.Key,
                        elementTypesByKey[contentData.ContentTypeKey].VariesByCulture() ? culture : null,
                        null));
                }

                foreach (BlockItemVariation alignedVariation in alignedVariations)
                {
                    blockValue.Expose.Add(alignedVariation);
                }
            }
        }

        blockValue.Expose = blockValue.Expose.DistinctBy(e => $"{e.ContentKey}.{e.Culture}.{e.Segment}").ToList();
    }

    private static bool VariesByCulture(BlockPropertyValue blockPropertyValue)
        => blockPropertyValue.Culture.IsNullOrWhiteSpace() is false;

    private static BlockPropertyValue? ValueToRetain(
        IEnumerable<BlockPropertyValue> blockPropertyValues,
        IGrouping<(string Alias, string? Segment), BlockPropertyValue> collapsingValues,
        string culture,
        string defaultIsoCode)
    {
        // an explicitly invariant value already covers the property, so the culture specific ones are leftovers
        var hasInvariantValue = blockPropertyValues.Any(blockPropertyValue
            => blockPropertyValue.Alias == collapsingValues.Key.Alias
               && blockPropertyValue.Segment == collapsingValues.Key.Segment
               && VariesByCulture(blockPropertyValue) is false);

        return hasInvariantValue
            ? null
            : collapsingValues.FirstOrDefault(blockPropertyValue => blockPropertyValue.Culture.InvariantEquals(culture))
              ?? collapsingValues.FirstOrDefault(blockPropertyValue => blockPropertyValue.Culture.InvariantEquals(defaultIsoCode));
    }
}
