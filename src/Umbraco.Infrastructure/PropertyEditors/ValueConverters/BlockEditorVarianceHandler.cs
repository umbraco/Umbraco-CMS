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
    private readonly IVariationContextAccessor _variationContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockEditorVarianceHandler"/> class.
    /// </summary>
    /// <param name="languageService">Service used to manage and retrieve language information for localization.</param>
    /// <param name="contentTypeService">Service used to manage and retrieve content type definitions.</param>
    /// <param name="variationContextAccessor">Accessor for the current variation context, used for culture and segment variations.</param>
    public BlockEditorVarianceHandler(ILanguageService languageService, IContentTypeService contentTypeService, IVariationContextAccessor variationContextAccessor)
    {
        _languageService = languageService;
        _contentTypeService = contentTypeService;
        _variationContextAccessor = variationContextAccessor;
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
            // An explicitly invariant value is the one matching the current schema, so it takes precedence over the
            // culture specific leftovers - see axiom 3 in block-element-level-variation.md, structure over data.
            var hasInvariantValue = blockPropertyValues.Any(blockPropertyValue
                => blockPropertyValue.Alias == group.Key.Alias
                   && blockPropertyValue.Segment == group.Key.Segment
                   && VariesByCulture(blockPropertyValue) is false);

            BlockPropertyValue? valueToRetain = hasInvariantValue
                ? null
                : ValueToRetain(group, culture, defaultIsoCode);
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
    [Obsolete("Please use the overload that aligns all property values of a block element. Scheduled for removal in Umbraco 19.")]
    public async Task<BlockPropertyValue?> AlignedPropertyVarianceAsync(BlockPropertyValue blockPropertyValue, IPublishedPropertyType propertyType, IPublishedElement owner)
    {
        var defaultCulture = await _languageService.GetDefaultIsoCodeAsync();

        if (owner.ContentType.VariesByCulture() is false
            && VariesByCulture(blockPropertyValue)
            && blockPropertyValue.Culture.InvariantEquals(defaultCulture) is false)
        {
            // variant property for a non-default language in an invariant context - do not use
            return null;
        }

        return Aligned(blockPropertyValue, propertyType, owner, blockPropertyValue.Culture, defaultCulture);
    }

    /// <summary>
    /// Aligns the property values of a block element for variance changes.
    /// </summary>
    /// <param name="blockPropertyValues">The block property values to align.</param>
    /// <param name="elementType">The published element type the values belong to.</param>
    /// <param name="owner">The owner element, which is either the content for block properties at the content level or the parent element for nested block properties.</param>
    /// <param name="culture">The culture of the owning property value, or <c>null</c> when the owning property does not vary by culture.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing the aligned block property values.</returns>
    /// <remarks>
    /// <para>Used for aligning variance changes when rendering content.</para>
    /// <para>
    /// This applies the same rule as <see cref="AlignPropertyVarianceAsync"/> does when editing: a property type that has
    /// become culture variant adopts <paramref name="culture"/>, and a property type that has become culture invariant
    /// retains a single value per alias and segment - an explicitly invariant value first, then the value for
    /// <paramref name="culture"/>, then the value for the default language.
    /// </para>
    /// <para>The supplied values are never modified; realigned values are returned as new instances.</para>
    /// </remarks>
    public async Task<IList<BlockPropertyValue>> AlignedPropertyVarianceAsync(
        IList<BlockPropertyValue> blockPropertyValues,
        IPublishedContentType elementType,
        IPublishedElement owner,
        string? culture)
    {
        var defaultCulture = await _languageService.GetDefaultIsoCodeAsync();
        var alignmentCulture = culture ?? defaultCulture;

        var alignedValues = new List<BlockPropertyValue>();
        foreach (IGrouping<(string Alias, string? Segment), BlockPropertyValue> group in blockPropertyValues
                     .GroupBy(blockPropertyValue => (blockPropertyValue.Alias, blockPropertyValue.Segment)))
        {
            IPublishedPropertyType? propertyType = elementType.GetPropertyType(group.Key.Alias);
            if (propertyType is null)
            {
                continue;
            }

            ContentVariation variation = owner.ContentType.Variations & propertyType.Variations;
            if (variation.VariesByCulture())
            {
                // The property type and the owning content both vary by culture, ensure culture variance accordingly.
                alignedValues.AddRange(group.Select(blockPropertyValue => Aligned(
                    blockPropertyValue,
                    propertyType,
                    owner,
                    VariesByCulture(blockPropertyValue) ? blockPropertyValue.Culture : alignmentCulture,
                    defaultCulture)));
                continue;
            }

            // Culture variance is not applicable, so it's safe to add the values that do not vary by culture.
            BlockPropertyValue[] invariantValues = group
                .Where(blockPropertyValue => VariesByCulture(blockPropertyValue) is false)
                .ToArray();
            if (invariantValues.Length > 0)
            {
                // Nothing to align here.
                alignedValues.AddRange(invariantValues.Select(blockPropertyValue
                    => Aligned(blockPropertyValue, propertyType, owner, culture: null, defaultCulture)));
                continue;
            }

            // A culture variation mismatch between the stored value and the current (effective) variance, likely caused
            // by a schema change. Only a single value can survive; prioritize an exact culture match, fallback to the
            // default culture.
            BlockPropertyValue? valueToRetain = ValueToRetain(group, alignmentCulture, defaultCulture);
            if (valueToRetain is not null)
            {
                alignedValues.Add(Aligned(valueToRetain, propertyType, owner, culture: null, defaultCulture));
            }
        }

        return alignedValues;
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
        var elementTypesByKey = blockValue
            .ContentData
            .Select(cd => cd.ContentTypeKey)
            .Distinct()
            .Select(_contentTypeService.Get)
            .WhereNotNull()
            .ToDictionary(c => c.Key);

        List<BlockItemData> contentDataToAlign = blockValue.Expose
            .Select(variation => ContentDataToAlign(blockValue, elementTypesByKey, variation))
            .WhereNotNull()
            .DistinctBy(contentData => contentData.Key)
            .ToList();

        // Remove expose entries that don't have matching entries in the block value's content data.
        var validContentKeys = blockValue.ContentData.Select(cd => cd.Key).ToHashSet();
        blockValue.Expose.RemoveAll(v => validContentKeys.Contains(v.ContentKey) is false);

        if (contentDataToAlign.Count > 0)
        {
            var contentKeysToAlign = contentDataToAlign.Select(cd => cd.Key).ToHashSet();
            List<BlockItemVariation> replacedVariations = blockValue.Expose
                .Where(v => contentKeysToAlign.Contains(v.ContentKey))
                .ToList();
            blockValue.Expose.RemoveAll(v => contentKeysToAlign.Contains(v.ContentKey));

            foreach (BlockItemData contentData in contentDataToAlign)
            {
                IContentType elementType = elementTypesByKey[contentData.ContentTypeKey];
                foreach (BlockItemVariation alignedVariation in AlignedExposeVariations(contentData, elementType, replacedVariations, culture))
                {
                    blockValue.Expose.Add(alignedVariation);
                }
            }
        }

        blockValue.Expose = blockValue.Expose.DistinctBy(e => $"{e.ContentKey}.{e.Culture}.{e.Segment}").ToList();
    }

    /// <summary>
    /// Determines whether an expose entry's block requires realignment, returning the block's content data when it does.
    /// </summary>
    private static BlockItemData? ContentDataToAlign(
        BlockValue blockValue,
        IReadOnlyDictionary<Guid, IContentType> elementTypesByKey,
        BlockItemVariation variation)
    {
        BlockItemData? contentData = blockValue.ContentData.FirstOrDefault(cd => cd.Key == variation.ContentKey);
        if (contentData is null
            || elementTypesByKey.TryGetValue(contentData.ContentTypeKey, out IContentType? elementType) is false
            || (variation.Culture is not null) == elementType.VariesByCulture())
        {
            return null;
        }

        var requiresAlignment = contentData.Values.Count == 0
                                || (variation.Culture is null && contentData.Values.Any(v => v.Culture is not null))
                                || (variation.Culture is not null && contentData.Values.All(v => v.Culture is null));

        return requiresAlignment ? contentData : null;
    }

    /// <summary>
    /// Builds the expose entries for a block, derived from the variance of its property values.
    /// </summary>
    private static IEnumerable<BlockItemVariation> AlignedExposeVariations(
        BlockItemData contentData,
        IContentType elementType,
        IEnumerable<BlockItemVariation> replacedVariations,
        string? culture)
    {
        var omitNullCulture = contentData.Values.Any(v => v.Culture is not null);
        List<BlockItemVariation> alignedVariations = contentData.Values
            .Where(v => omitNullCulture is false || v.Culture is not null)
            .DistinctBy(v => v.Culture + v.Segment)
            .Select(v => new BlockItemVariation(contentData.Key, v.Culture, v.Segment))
            .ToList();

        if (alignedVariations.Count > 0)
        {
            return alignedVariations;
        }

        // a block without property values has no value variance to align against, so keep it exposed for the element
        // type's variance, retaining the segments it was exposed for
        var alignedCulture = elementType.VariesByCulture() ? culture : null;
        return replacedVariations
            .Where(v => v.ContentKey == contentData.Key)
            .Select(v => v.Segment)
            .DefaultIfEmpty(null)
            .Distinct()
            .Select(segment => new BlockItemVariation(contentData.Key, alignedCulture, segment));
    }

    private static bool VariesByCulture(BlockPropertyValue blockPropertyValue)
        => blockPropertyValue.Culture.IsNullOrWhiteSpace() is false;

    private static bool VariesBySegment(BlockPropertyValue blockPropertyValue)
        => blockPropertyValue.Segment.IsNullOrWhiteSpace() is false;

    /// <summary>
    /// Determines which of a property's culture specific values survives the property type becoming culture invariant.
    /// </summary>
    private static BlockPropertyValue? ValueToRetain(
        IEnumerable<BlockPropertyValue> cultureSpecificValues,
        string culture,
        string defaultIsoCode)
        => cultureSpecificValues.FirstOrDefault(blockPropertyValue => blockPropertyValue.Culture.InvariantEquals(culture))
           ?? cultureSpecificValues.FirstOrDefault(blockPropertyValue => blockPropertyValue.Culture.InvariantEquals(defaultIsoCode));

    /// <summary>
    /// Realigns a block property value to the variance the element type currently expects, contextualizing the
    /// variance the value does not carry itself with the current variation context.
    /// </summary>
    /// <param name="blockPropertyValue">The value to realign.</param>
    /// <param name="propertyType">The published property type the value belongs to.</param>
    /// <param name="owner">The owner element the value is rendered for.</param>
    /// <param name="culture">The culture the value has been aligned to, or <c>null</c> when it has been aligned as invariant.</param>
    /// <param name="defaultCulture">The default language, applied when the variation context holds no culture.</param>
    private BlockPropertyValue Aligned(
        BlockPropertyValue blockPropertyValue,
        IPublishedPropertyType propertyType,
        IPublishedElement owner,
        string? culture,
        string defaultCulture)
    {
        VariationContext variationContext = _variationContextAccessor.VariationContext ?? new VariationContext();

        // A value aligned as invariant is still rendered by a property type that varies whenever the owning content
        // does not vary by what the element type does, so it adopts the variance of the current context.
        var alignedCulture = propertyType.Variations.VariesByCulture()
            ? culture.IfNullOrWhiteSpace(variationContext.Culture.IfNullOrWhiteSpace(defaultCulture))
            : null;
        var alignedSegment = propertyType.Variations.VariesBySegment()
            ? owner.ContentType.VariesBySegment() is false && VariesBySegment(blockPropertyValue)
                ? variationContext.Segment
                : blockPropertyValue.Segment.IfNullOrWhiteSpace(variationContext.Segment)
            : null;

        return new BlockPropertyValue
        {
            Alias = blockPropertyValue.Alias,
            Culture = alignedCulture,
            Segment = alignedSegment,
            Value = blockPropertyValue.Value,
            PropertyType = blockPropertyValue.PropertyType,
        };
    }
}
