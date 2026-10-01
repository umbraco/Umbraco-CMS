using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Search.Indexing.Collection;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using IndexValue = Umbraco.Cms.Core.Search.Indexing.IndexValue;

namespace Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;

/// <summary>
/// Base class for property value handlers of block-based editors (block list, block grid, single block, rich text).
/// Recursively indexes the property values of the blocks' content, accumulating them per culture/segment variation.
/// </summary>
internal abstract class BlockEditorPropertyValueHandler : IPropertyValueHandler
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IContentTypeService _contentTypeService;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly PropertyValueHandlerCollection _propertyValueHandlerCollection;
    private readonly ILogger<BlockEditorPropertyValueHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockEditorPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize the block property's stored value.</param>
    /// <param name="contentTypeService">The service used to resolve the contained blocks' element types.</param>
    /// <param name="propertyEditorCollection">The property editor collection used to resolve each contained property's editor.</param>
    /// <param name="propertyValueHandlerCollection">The property value handler collection used to index each contained property's value.</param>
    /// <param name="logger">The logger used to record diagnostic information when indexing blocks.</param>
    protected BlockEditorPropertyValueHandler(
        IJsonSerializer jsonSerializer,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        PropertyValueHandlerCollection propertyValueHandlerCollection,
        ILogger<BlockEditorPropertyValueHandler> logger)
    {
        _jsonSerializer = jsonSerializer;
        _contentTypeService = contentTypeService;
        _propertyEditorCollection = propertyEditorCollection;
        _propertyValueHandlerCollection = propertyValueHandlerCollection;
        _logger = logger;
    }

    /// <inheritdoc />
    public abstract bool CanHandle(IPropertyType propertyType);

    /// <inheritdoc />
    public virtual IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        BlockValue? blockValue = ParsePropertyValue(property, culture, segment, published);
        if (blockValue is null || blockValue.ContentData.Count == 0)
        {
            return [];
        }

        Dictionary<(string? Culture, string? Segment), CumulativeIndexValue> blockIndexValues = GetCumulativeIndexValues(blockValue, property, culture, segment, published, contentContext);
        return ToIndexFields(blockIndexValues, property.Alias);
    }

    private BlockValue? ParsePropertyValue(IProperty property, string? culture, string? segment, bool published)
    {
        var value = property.GetValue(culture, segment, published) as string;
        return value?.DetectIsJson() is true
            ? _jsonSerializer.Deserialize<BlockValue>(value)
            : null;
    }

    /// <summary>
    /// Builds the cumulative index values for all contained blocks of a block property, keyed by culture/segment variation.
    /// </summary>
    /// <param name="blockValue">The parsed block property value.</param>
    /// <param name="property">The block property.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="segment">The requested segment.</param>
    /// <param name="published">Whether to index the published or draft values.</param>
    /// <param name="contentContext">The content the block property belongs to.</param>
    /// <returns>The cumulative index values per culture/segment variation.</returns>
    protected Dictionary<(string? Culture, string? Segment), CumulativeIndexValue> GetCumulativeIndexValues(
        BlockValue blockValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IContentBase contentContext)
        => GetCumulativeIndexValues(blockValue.ContentData, blockValue.Expose, property, culture, segment, published, contentContext);

    /// <summary>
    /// Builds the cumulative index values for the given block content items, keyed by culture/segment variation.
    /// </summary>
    /// <param name="items">The block content items to index.</param>
    /// <param name="expose">The block variations exposed for publishing.</param>
    /// <param name="property">The block property.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="segment">The requested segment.</param>
    /// <param name="published">Whether to index the published or draft values.</param>
    /// <param name="contentContext">The content the block property belongs to.</param>
    /// <returns>The cumulative index values per culture/segment variation.</returns>
    protected Dictionary<(string? Culture, string? Segment), CumulativeIndexValue> GetCumulativeIndexValues(
        IList<BlockItemData> items,
        IList<BlockItemVariation> expose,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IContentBase contentContext)
    {
        // block level variance can cause invariant culture to expand into multiple concrete cultures
        var propertyCultures = GetPropertyCultures(property.PropertyType, culture, published, contentContext);

        // load all the contained element types up front
        var elementTypesByKey = _contentTypeService
            .GetMany(items.Select(cd => cd.ContentTypeKey).Distinct())
            .ToDictionary(c => c.Key);

        // these are the cumulative index values (for all contained blocks) per contained variation
        var cumulativeIndexValuesByVariation = new Dictionary<(string? Culture, string? Segment), CumulativeIndexValue>();

        foreach (BlockItemData contentData in items)
        {
            Dictionary<string, IPropertyType>? propertyTypesByAlias = GetPropertyTypesByAlias(contentData.ContentTypeKey, elementTypesByKey);
            if (propertyTypesByAlias is null)
            {
                continue;
            }

            foreach (var propertyCulture in propertyCultures)
            {
                foreach (BlockPropertyValue blockPropertyValue in contentData.Values.Where(value => value.Culture.InvariantEquals(propertyCulture)))
                {
                    if (published
                        && propertyCulture is not null
                        && expose.Any(e =>
                            e.ContentKey == contentData.Key &&
                            e.Culture.InvariantEquals(blockPropertyValue.Culture)) is false)
                    {
                        // un-exposed blocks should not be included in published indexing
                        continue;
                    }

                    if (propertyTypesByAlias.TryGetValue(blockPropertyValue.Alias, out IPropertyType? propertyType) is false)
                    {
                        // this is to be expected, if the property type has been removed from
                        // the element type after the block creation
                        continue;
                    }

                    IDataEditor? editor = _propertyEditorCollection[propertyType.PropertyEditorAlias];
                    if (editor is null)
                    {
                        _logger.LogDebug(
                            "No property editor found for property editor alias {propertyEditorAlias} - skipped indexing of property value.",
                            propertyType.PropertyEditorAlias);
                        continue;
                    }

                    var blockProperty = new Property(propertyType);
                    if (propertyType.VariesByCulture() && propertyCulture is null)
                    {
                        continue;
                    }

                    blockProperty.SetValue(blockPropertyValue.Value, propertyCulture, segment);
                    if (published)
                    {
                        blockProperty.PublishValues(propertyCulture ?? "*", segment ?? "*");
                    }

                    IPropertyValueHandler? blockPropertyValueHandler = _propertyValueHandlerCollection.GetPropertyValueHandler(propertyType);
                    if (blockPropertyValueHandler is null)
                    {
                        _logger.LogDebug(
                            "No property value handler found for property editor alias {propertyEditorAlias} - skipped indexing of property value.",
                            propertyType.PropertyEditorAlias);
                        continue;
                    }

                    IndexField[] blockPropertyIndexFields = blockPropertyValueHandler
                        .GetIndexFields(blockProperty, propertyCulture, segment, published, contentContext)
                        .ToArray();

                    foreach (IndexField blockPropertyIndexField in blockPropertyIndexFields)
                    {
                        // an invariant nested property value (e.g. from a block whose elements don't vary) belongs
                        // to the culture/segment of the containing property when that property itself varies -
                        // it must not collapse to invariant (null) in that case.
                        (string? Culture, string? Segment) variation = (blockPropertyIndexField.Culture ?? culture, blockPropertyIndexField.Segment ?? segment);

                        if (cumulativeIndexValuesByVariation.TryGetValue(variation, out CumulativeIndexValue? blockIndexValue) is false)
                        {
                            blockIndexValue = new CumulativeIndexValue();
                            cumulativeIndexValuesByVariation.Add(variation, blockIndexValue);
                        }

                        AmendCumulativeIndexValue(blockIndexValue, blockPropertyIndexField.Value);
                    }
                }
            }
        }

        return cumulativeIndexValuesByVariation;
    }

    /// <summary>
    /// Merges an index value's field values into a cumulative index value.
    /// </summary>
    /// <param name="cumulativeIndexValue">The cumulative index value to merge into.</param>
    /// <param name="indexValue">The index value to merge from.</param>
    protected void AmendCumulativeIndexValue(CumulativeIndexValue cumulativeIndexValue, IndexValue indexValue)
    {
        cumulativeIndexValue.TextsR1.AddRange(indexValue.TextsR1.EmptyNull());
        cumulativeIndexValue.TextsR2.AddRange(indexValue.TextsR2.EmptyNull());
        cumulativeIndexValue.TextsR3.AddRange(indexValue.TextsR3.EmptyNull());
        cumulativeIndexValue.Texts.AddRange(indexValue.Texts.EmptyNull());
        cumulativeIndexValue.Keywords.AddRange(indexValue.Keywords.EmptyNull());
        cumulativeIndexValue.Integers.AddRange(indexValue.Integers.EmptyNull());
        cumulativeIndexValue.Decimals.AddRange(indexValue.Decimals.EmptyNull());
        cumulativeIndexValue.DateTimeOffsets.AddRange(indexValue.DateTimeOffsets.EmptyNull());
    }

    /// <summary>
    /// Converts a cumulative index value into an index value, or null if it carries no data.
    /// </summary>
    /// <param name="cumulativeIndexValue">The cumulative index value to convert.</param>
    /// <returns>The resulting index value, or null if it is empty.</returns>
    protected IndexValue? ToIndexValue(CumulativeIndexValue cumulativeIndexValue)
        => cumulativeIndexValue.TextsR1.Count > 0
           || cumulativeIndexValue.TextsR2.Count > 0
           || cumulativeIndexValue.TextsR3.Count > 0
           || cumulativeIndexValue.Texts.Count > 0
           || cumulativeIndexValue.Keywords.Count > 0
           || cumulativeIndexValue.Integers.Count > 0
           || cumulativeIndexValue.Decimals.Count > 0
           || cumulativeIndexValue.DateTimeOffsets.Count > 0
            ? new IndexValue
            {
                TextsR1 = cumulativeIndexValue.TextsR1.NullIfEmpty(),
                TextsR2 = cumulativeIndexValue.TextsR2.NullIfEmpty(),
                TextsR3 = cumulativeIndexValue.TextsR3.NullIfEmpty(),
                Texts = cumulativeIndexValue.Texts.NullIfEmpty(),
                Keywords = cumulativeIndexValue.Keywords.NullIfEmpty(),
                Integers = cumulativeIndexValue.Integers.NullIfEmpty(),
                Decimals = cumulativeIndexValue.Decimals.NullIfEmpty(),
                DateTimeOffsets = cumulativeIndexValue.DateTimeOffsets.NullIfEmpty(),
            }
            : null;

    /// <summary>
    /// Converts cumulative index values per culture/segment variation into index fields for the given property alias.
    /// </summary>
    /// <param name="cumulativeIndexValues">The cumulative index values per culture/segment variation.</param>
    /// <param name="propertyAlias">The alias of the property being indexed.</param>
    /// <returns>The resulting index fields, omitting any empty variations.</returns>
    protected IEnumerable<IndexField> ToIndexFields(Dictionary<(string? Culture, string? Segment), CumulativeIndexValue> cumulativeIndexValues, string propertyAlias)
        => cumulativeIndexValues.Select(kvp
                => ToIndexValue(kvp.Value) is { } indexValue
                    ? new IndexField(
                        propertyAlias,
                        indexValue,
                        kvp.Key.Culture,
                        kvp.Key.Segment)
                    : null)
            .WhereNotNull()
            .ToArray();

    private string?[] GetPropertyCultures(IPropertyType propertyType, string? requestedCulture, bool published, IContentBase contentContext)
    {
        // block level variance can cause invariant culture to expand into multiple concrete cultures
        var propertyCultures = propertyType.VariesByCulture()
            ? [requestedCulture]
            : contentContext.ContentType.VariesByCulture()
                ? published
                    ? contentContext.PublishedCultures()
                    : contentContext.AvailableCultures().ToArray()
                : [requestedCulture];
        if (propertyCultures.Contains(null) is false)
        {
            // don't forget the invariant culture
            propertyCultures = propertyCultures.Union([null]).ToArray();
        }

        return propertyCultures;
    }

    private Dictionary<string, IPropertyType>? GetPropertyTypesByAlias(Guid elementTypeKey, Dictionary<Guid, IContentType> elementTypes)
    {
        if (elementTypes.TryGetValue(elementTypeKey, out IContentType? elementType) is false)
        {
            return null;
        }

        // it's perfectly valid for a nested property type to be invariant even if the containing block (or the
        // property it belongs to) varies - for instance in a block list, the list itself can vary while the
        // elements within it are invariant. the nested property types' own variation must therefore be respected
        // as-is, not coerced to match the requested culture/segment.
        return elementType
            .CompositionPropertyTypes
            .ToDictionary(x => x.Alias);
    }

    /// <summary>
    /// Represents the deserialized value of a block property.
    /// </summary>
    protected class BlockValue
    {
        /// <summary>
        /// Gets the block content items.
        /// </summary>
        public required List<BlockItemData> ContentData { get; init; }

        /// <summary>
        /// Gets the block variations exposed for publishing.
        /// </summary>
        public required List<BlockItemVariation> Expose { get; init; }
    }

    /// <summary>
    /// Accumulates index field values across all blocks contained in a block property, for a single culture/segment variation.
    /// </summary>
    protected record CumulativeIndexValue
    {
        /// <summary>
        /// Gets the accumulated heading-1 relevance texts.
        /// </summary>
        public List<string> TextsR1 { get; } = [];

        /// <summary>
        /// Gets the accumulated heading-2 relevance texts.
        /// </summary>
        public List<string> TextsR2 { get; } = [];

        /// <summary>
        /// Gets the accumulated heading-3 relevance texts.
        /// </summary>
        public List<string> TextsR3 { get; } = [];

        /// <summary>
        /// Gets the accumulated body texts.
        /// </summary>
        public List<string> Texts { get; } = [];

        /// <summary>
        /// Gets the accumulated keywords.
        /// </summary>
        public List<string> Keywords { get; } = [];

        /// <summary>
        /// Gets the accumulated integers.
        /// </summary>
        public List<int> Integers { get; } = [];

        /// <summary>
        /// Gets the accumulated decimals.
        /// </summary>
        public List<decimal> Decimals { get; } = [];

        /// <summary>
        /// Gets the accumulated dates.
        /// </summary>
        public List<DateTimeOffset> DateTimeOffsets { get; } = [];
    }
}
