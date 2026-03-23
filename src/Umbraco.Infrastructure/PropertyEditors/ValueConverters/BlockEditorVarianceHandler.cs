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
    /// <param name="culture">The culture being handled (null if invariant).</param>
    /// <returns>A task that represents the asynchronous operation, containing the aligned block property values.</returns>
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
    /// <returns>A task representing the asynchronous operation. The task result contains the aligned <see cref="BlockPropertyValue"/>, or <c>null</c> if alignment is not applicable.</returns>
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
    /// Aligns a block value for variance changes and adds language fallback entries.
    /// </summary>
    /// <param name="blockValue">The block property value to align for variance.</param>
    /// <param name="owner">The owner element, which is either the content for block properties at the content level or the parent element for nested block properties.</param>
    /// <param name="element">The block element containing the property.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing the aligned <see cref="BlockItemVariation"/> instances for the specified block element.</returns>
    /// <remarks>
    /// <para>Used for aligning block item variations according to variance (such as culture or segment) when rendering content.</para>
    /// <para>Additionally, for culture-variant blocks, this method walks the language fallback chain (<see cref="ILanguage.FallbackIsoCode"/>) and
    /// adds synthetic expose entries for languages that fall back to an already-exposed culture. This ensures blocks are visible when requesting a
    /// language that has a fallback path to the culture the block was exposed for.</para>
    /// </remarks>
    [Obsolete("Use the overload accepting allLanguages. Scheduled for removal in Umbraco 19.")]
    public async Task<IEnumerable<BlockItemVariation>> AlignedExposeVarianceAsync(BlockValue blockValue, IPublishedElement owner, IPublishedElement element)
    {
        IReadOnlyList<ILanguage> allLanguages = (await _languageService.GetAllAsync()).ToList();
        return await AlignedExposeVarianceAsync(blockValue, owner, element, allLanguages);
    }

    /// <summary>
    /// Aligns a block value for variance changes and adds language fallback entries.
    /// </summary>
    /// <param name="blockValue">The block property value to align for variance.</param>
    /// <param name="owner">The owner element, which is either the content for block properties at the content level or the parent element for nested block properties.</param>
    /// <param name="element">The block element containing the property.</param>
    /// <param name="allLanguages">All configured languages, used for walking fallback chains.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing the aligned <see cref="BlockItemVariation"/> instances for the specified block element.</returns>
    /// <remarks>
    /// <para>Used for aligning block item variations according to variance (such as culture or segment) when rendering content.</para>
    /// <para>Additionally, for culture-variant blocks, this method walks the language fallback chain (<see cref="ILanguage.FallbackIsoCode"/>) and
    /// adds synthetic expose entries for languages that fall back to an already-exposed culture. This ensures blocks are visible when requesting a
    /// language that has a fallback path to the culture the block was exposed for.</para>
    /// </remarks>
    public async Task<IEnumerable<BlockItemVariation>> AlignedExposeVarianceAsync(BlockValue blockValue, IPublishedElement owner, IPublishedElement element, IReadOnlyList<ILanguage> allLanguages)
    {
        BlockItemVariation[] blockVariations = blockValue.Expose.Where(v => v.ContentKey == element.Key).ToArray();
        if (blockVariations.Any() is false)
        {
            return blockVariations;
        }

        // In case of mismatch in culture variation for block value variation:
        // - if the expected variation is by culture but all expose entries are invariant, assign the default culture
        // - if the expected variation is invariant but all expose entries have cultures, use the default culture entry as invariant
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

        // For culture-variant blocks, add expose entries for languages whose fallback chains lead to an already-exposed culture.
        if (exposeVariation.VariesByCulture())
        {
            IReadOnlyList<BlockItemVariation> fallbackEntries = GetLanguageFallbackExposeEntries(blockVariations, element.Key, allLanguages);
            if (fallbackEntries.Count > 0)
            {
                return blockVariations.Concat(fallbackEntries);
            }
        }

        return blockVariations;
    }

    /// <summary>
    /// Walks the language fallback chains and returns synthetic expose entries for languages
    /// that are not directly exposed but have a fallback path to an exposed culture.
    /// </summary>
    private static IReadOnlyList<BlockItemVariation> GetLanguageFallbackExposeEntries(
        BlockItemVariation[] blockVariations,
        Guid elementKey,
        IReadOnlyList<ILanguage> allLanguages)
    {
        // Track existing exposure by (culture, segment) so we only create fallback entries for specific segments that
        // are missing — not skip an entire language just because it has some segments already exposed.
        var cultureSegmentComparer = new CultureSegmentEqualityComparer();
        var existingExposure = new HashSet<(string? Culture, string? Segment)>(
            blockVariations.Select(v => (v.Culture, v.Segment)), cultureSegmentComparer);
        var exposedCultures = new HashSet<string?>(
            blockVariations.Select(v => v.Culture), StringComparer.OrdinalIgnoreCase);

        var languagesByIsoCode = allLanguages.ToDictionary(l => l.IsoCode, StringComparer.OrdinalIgnoreCase);
        var fallbackEntries = new List<BlockItemVariation>();

        foreach (ILanguage language in allLanguages)
        {
            // Walk the fallback chain for this language.
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ILanguage? current = language;
            while (current?.FallbackIsoCode is not null)
            {
                if (visited.Add(current.FallbackIsoCode) is false)
                {
                    break;
                }

                if (exposedCultures.Contains(current.FallbackIsoCode))
                {
                    foreach (BlockItemVariation fallbackVariation in blockVariations
                        .Where(v => string.Equals(v.Culture, current.FallbackIsoCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        (string IsoCode, string? Segment) candidate = (language.IsoCode, fallbackVariation.Segment);
                        if (existingExposure.Contains(candidate) is false)
                        {
                            fallbackEntries.Add(new BlockItemVariation(elementKey, language.IsoCode, fallbackVariation.Segment));
                            existingExposure.Add(candidate);
                        }
                    }

                    break;
                }

                languagesByIsoCode.TryGetValue(current.FallbackIsoCode, out current);
            }
        }

        return fallbackEntries;
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

            if ((variation.Culture is not null) == elementType.VariesByCulture())
            {
                continue;
            }

            if ((variation.Culture is null && contentData.Values.Any(v => v.Culture is not null)) ||
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
                foreach (BlockPropertyValue value in contentData.Values
                    .Where(v => omitNullCulture is false || v.Culture is not null)
                    .DistinctBy(v => v.Culture + v.Segment))
                {
                    blockValue.Expose.Add(new BlockItemVariation(contentData.Key, value.Culture, value.Segment));
                }
            }
        }

        blockValue.Expose = blockValue.Expose.DistinctBy(e => $"{e.ContentKey}.{e.Culture}.{e.Segment}").ToList();
    }

    private static bool VariesByCulture(BlockPropertyValue blockPropertyValue)
        => blockPropertyValue.Culture.IsNullOrWhiteSpace() is false;

    private sealed class CultureSegmentEqualityComparer : IEqualityComparer<(string? Culture, string? Segment)>
    {
        public bool Equals((string? Culture, string? Segment) x, (string? Culture, string? Segment) y)
            => StringComparer.OrdinalIgnoreCase.Equals(x.Culture, y.Culture)
               && StringComparer.OrdinalIgnoreCase.Equals(x.Segment, y.Segment);

        public int GetHashCode((string? Culture, string? Segment) obj)
            => HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Culture ?? string.Empty),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Segment ?? string.Empty));
    }
}
