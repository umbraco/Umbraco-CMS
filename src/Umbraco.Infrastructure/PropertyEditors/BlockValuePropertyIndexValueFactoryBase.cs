using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class BlockValuePropertyIndexValueFactoryBase<TSerialized> : JsonPropertyIndexValueFactoryBase<TSerialized>
{
    private readonly PropertyEditorCollection _propertyEditorCollection;

    protected BlockValuePropertyIndexValueFactoryBase(
        PropertyEditorCollection propertyEditorCollection,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(jsonSerializer, indexingSettings)
    {
        _propertyEditorCollection = propertyEditorCollection;
    }

    protected override IEnumerable<IndexValue> Handle(
        TSerialized deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        var result = new List<IndexValue>();
        var propertyTypeDictionaries = new Dictionary<Guid, Dictionary<string, IPropertyType>>();

        var index = 0;
        foreach (RawDataItem rawData in GetDataItems(deserializedPropertyValue, published))
        {
            if (contentTypeDictionary.TryGetValue(rawData.ContentTypeKey, out IContentType? contentType) is false)
            {
                continue;
            }

            if (propertyTypeDictionaries.TryGetValue(contentType.Key, out Dictionary<string, IPropertyType>? propertyTypeDictionary) is false)
            {
                propertyTypeDictionary =
                    contentType
                        .CompositionPropertyTypes
                        .Select(propertyType => WithParentVariation(propertyType, culture, segment))
                        .ToDictionary(x => x.Alias);
                propertyTypeDictionaries[contentType.Key] = propertyTypeDictionary;
            }

            result.AddRange(GetNestedResults(
                $"{property.Alias}.items[{index}]",
                culture,
                segment,
                published,
                propertyTypeDictionary,
                rawData,
                availableCultures,
                contentTypeDictionary));

            index++;
        }

        return RenameKeysToEnsureRawSegmentsIsAPrefix(result);
    }

    /// <summary>
    /// Gets the property type to index a nested property with, varying by culture and segment if the parent value does.
    /// </summary>
    /// <remarks>
    /// It's perfectly valid to have a nested property type that's set to invariant even if the parent varies. For
    /// instance in a block list, the list itself can vary, but the elements can be invariant, at the same time.
    /// The element type is shared by every value indexed with it, so a copy is returned rather than changing it.
    /// </remarks>
    private static IPropertyType WithParentVariation(IPropertyType propertyType, string? culture, string? segment)
    {
        ContentVariation variations = propertyType.Variations;
        if (culture is not null)
        {
            variations |= ContentVariation.Culture;
        }

        if (segment is not null)
        {
            variations |= ContentVariation.Segment;
        }

        if (variations == propertyType.Variations)
        {
            return propertyType;
        }

        var nestedPropertyType = (IPropertyType)propertyType.DeepClone();
        nestedPropertyType.Variations = variations;
        return nestedPropertyType;
    }

    /// <summary>
    /// Rename keys that count the RAW-constant, to ensure the RAW-constant is a prefix.
    /// </summary>
    private IEnumerable<IndexValue> RenameKeysToEnsureRawSegmentsIsAPrefix(
        List<IndexValue> indexContent)
    {
        foreach (IndexValue indexValue in indexContent)
        {
            // Tests if key includes the RawFieldPrefix and it is not in the start
            if (indexValue.FieldName.AsSpan(1).Contains(UmbracoExamineFieldNames.RawFieldPrefix, StringComparison.Ordinal))
            {
                indexValue.FieldName = UmbracoExamineFieldNames.RawFieldPrefix +
                                       indexValue.FieldName.Replace(UmbracoExamineFieldNames.RawFieldPrefix, string.Empty);
            }
        }

        return indexContent;
    }

    /// <summary>
    /// Get the data items of a parent item. E.g. block list have contentData.
    /// </summary>
    protected abstract IEnumerable<RawDataItem> GetDataItems(TSerialized input, bool published);

    /// <summary>
    /// Unwraps block item data as data items.
    /// </summary>
    protected IEnumerable<RawDataItem> GetDataItems(IList<BlockItemData> contentData, IList<BlockItemVariation> expose, bool published)
    {
        if (published is false)
        {
            return contentData.Select(ToRawData);
        }

        var indexData = new List<RawDataItem>();
        foreach (BlockItemData blockItemData in contentData)
        {
            var exposedCultures = expose
                .Where(e => e.ContentKey == blockItemData.Key)
                .Select(e => e.Culture)
                .ToArray();

            if (exposedCultures.Any() is false)
            {
                continue;
            }

            if (exposedCultures.Contains(null)
                || exposedCultures.ContainsAll(blockItemData.Values.Select(v => v.Culture)))
            {
                indexData.Add(ToRawData(blockItemData));
                continue;
            }

            indexData.Add(
                ToRawData(
                    blockItemData.ContentTypeKey,
                    blockItemData.Values.Where(value => value.Culture is null || exposedCultures.Contains(value.Culture))));
        }

        return indexData;
    }

    /// <summary>
    /// Index a key with the name of the property, using the relevant content of all the children.
    /// </summary>
    protected override IEnumerable<IndexValue> HandleResume(
        List<IndexValue> indexedContent,
        IProperty property,
        string? culture,
        string? segment,
        bool published)
    {
        var indexedCultures = indexedContent
            .DistinctBy(v => v.Culture)
            .Select(v => v.Culture)
            .WhereNotNull()
            .ToArray();
        var cultures = indexedCultures.Any()
            ? indexedCultures
            : new string?[] { culture };

        return cultures.Select(c => new IndexValue
        {
            Culture = c, FieldName = property.Alias, Values = [GetResumeFromAllContent(indexedContent, c)]
        });
    }

    /// <summary>
    /// Gets a resume as string of all the content in this nested type.
    /// </summary>
    /// <param name="indexedContent">All the indexed content for this property.</param>
    /// <param name="culture">The culture to get the resume for.</param>
    /// <returns>the string with all relevant content from </returns>
    private static string GetResumeFromAllContent(List<IndexValue> indexedContent, string? culture)
    {
        var stringBuilder = new StringBuilder();
        foreach (IndexValue indexValue in indexedContent.Where(v => v.Culture == culture || v.Culture is null))
        {
            // Ignore Raw fields
            if (indexValue.FieldName.Contains(UmbracoExamineFieldNames.RawFieldPrefix))
            {
                continue;
            }

            foreach (var value in indexValue.Values)
            {
                if (value is not null)
                {
                    stringBuilder.AppendLine(value.ToString());
                }
            }
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    ///  Gets the content to index for the nested type. E.g. Block list, Nested Content, etc..
    /// </summary>
    private IEnumerable<IndexValue> GetNestedResults(
        string keyPrefix,
        string? culture,
        string? segment,
        bool published,
        IDictionary<string, IPropertyType> propertyTypeDictionary,
        RawDataItem rawData,
        IEnumerable<string> availableCultures,
        IDictionary<Guid,IContentType> contentTypeDictionary)
    {
        var rawDataCultures = rawData.Properties.Select(property => property.Culture).Distinct().WhereNotNull().ToArray();
        foreach (RawPropertyData rawPropertyData in rawData.Properties)
        {
            if (propertyTypeDictionary.TryGetValue(rawPropertyData.Alias, out IPropertyType? propertyType))
            {
                IDataEditor? editor = _propertyEditorCollection[propertyType.PropertyEditorAlias];
                if (editor is null)
                {
                    continue;
                }

                var propertyCulture = rawPropertyData.Culture ?? culture;

                if (propertyType.VariesByCulture() is false && propertyCulture is not null)
                {
                    // culture variance mismatch - the stored property value varies by culture, but the element property
                    // does not. this is likely due to element (or element property) variation changes since last edit.
                    // we'll ignore this to avoid polluting the indexes with cross-variant values, as the same property
                    // might have a value in multiple languages.
                    continue;
                }

                if (propertyType.VariesByCulture() && propertyCulture is null)
                {
                    // The stored value has no culture, likely due to a change of the element property's variance. It's indexed as for the
                    // last available culture, which retains behaviour observed before the optimisation in #24263.
                    // Arguably this should be spread across all cultures.
                    propertyCulture = availableCultures.LastOrDefault();
                    if (propertyCulture is null)
                    {
                        continue;
                    }
                }

                IProperty subProperty = new Property(propertyType);
                subProperty.SetValue(rawPropertyData.Value, propertyCulture, segment);
                if (published)
                {
                    subProperty.PublishValues(propertyCulture ?? "*", segment ?? "*");
                }

                IEnumerable<IndexValue> indexValues = editor.PropertyIndexValueFactory.GetIndexValues(subProperty, propertyCulture, segment, published, availableCultures, contentTypeDictionary);
                foreach (IndexValue indexValue in indexValues)
                {
                    indexValue.FieldName = $"{keyPrefix}.{indexValue.FieldName}";

                    if (indexValue.Culture is null && rawDataCultures.Any())
                    {
                        foreach (var rawDataCulture in rawDataCultures)
                        {
                            yield return new IndexValue
                            {
                                Culture = rawDataCulture,
                                FieldName = indexValue.FieldName,
                                Values = indexValue.Values
                            };
                        }
                    }
                    else
                    {
                        indexValue.Culture = rawDataCultures.Any() ? indexValue.Culture : null;
                        yield return indexValue;
                    }
                }
            }
        }
    }

    private RawDataItem ToRawData(BlockItemData blockItemData)
        => ToRawData(blockItemData.ContentTypeKey, blockItemData.Values);

    private RawDataItem ToRawData(Guid contentTypeKey, IEnumerable<BlockPropertyValue> values)
        => new()
        {
            ContentTypeKey = contentTypeKey,
            Properties = values.Select(value => new RawPropertyData
            {
                Alias = value.Alias,
                Culture = value.Culture,
                Value = value.Value
            }).ToArray()
        };

    protected class RawDataItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the content type.
        /// </summary>
        public required Guid ContentTypeKey { get; init; }

        /// <summary>
        /// Gets or sets the collection of raw property data associated with this block item.
        /// </summary>
        public required IEnumerable<RawPropertyData> Properties { get; init; }
    }

    protected class RawPropertyData
    {
        /// <summary>Gets or sets the alias of the raw property data.</summary>
        public required string Alias { get; init; }

        /// <summary>
        /// Gets the unprocessed or original value of the property before any transformation or indexing.
        /// </summary>
        public required object? Value { get; init; }

        /// <summary>
        /// Gets or sets the culture identifier (e.g., "en-US") associated with the raw property data, or <c>null</c> if invariant.
        /// </summary>
        public required string? Culture { get; init; }
    }
}
