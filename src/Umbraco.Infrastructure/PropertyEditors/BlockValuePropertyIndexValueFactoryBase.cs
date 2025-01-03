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

        var index = 0;
        foreach (RawDataItem rawData in GetDataItems(deserializedPropertyValue, published))
        {
            if (contentTypeDictionary.TryGetValue(rawData.ContentTypeKey, out IContentType? contentType) is false)
            {
                continue;
            }

            var propertyTypeDictionary =
                contentType
                    .CompositionPropertyTypes
                    .Select(propertyType =>
                    {
                        // We want to ensure that the nested properties are set vary by culture if the parent is
                        // This is because it's perfectly valid to have a nested property type that's set to invariant even if the parent varies.
                        // For instance in a block list, the list it self can vary, but the elements can be invariant, at the same time.
                        if (culture is not null)
                        {
                            propertyType.Variations |= ContentVariation.Culture;
                        }

                        if (segment is not null)
                        {
                            propertyType.Variations |= ContentVariation.Segment;
                        }

                        return propertyType;
                    })
                    .ToDictionary(x => x.Alias);

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
    /// Rename keys that count the RAW-constant, to ensure the RAW-constant is a prefix.
    /// </summary>
    private IEnumerable<IndexValue> RenameKeysToEnsureRawSegmentsIsAPrefix(
        List<IndexValue> indexContent)
    {
        foreach (IndexValue indexValue in indexContent)
        {
            // Tests if key includes the RawFieldPrefix and it is not in the start
            if (indexValue.FieldName.Substring(1).Contains(UmbracoExamineFieldNames.RawFieldPrefix))
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
        foreach (RawPropertyData rawPropertyData in rawData.Properties)
        {
            if (propertyTypeDictionary.TryGetValue(rawPropertyData.Alias, out IPropertyType? propertyType))
            {
                IDataEditor? editor = _propertyEditorCollection[propertyType.PropertyEditorAlias];
                if (editor is null)
                {
                    continue;
                }

                IProperty subProperty = new Property(propertyType);
                IEnumerable<IndexValue> indexValues = null!;

                var propertyCulture = rawPropertyData.Culture ?? culture;

                if (propertyType.VariesByCulture() && propertyCulture is null)
                {
                    foreach (var availableCulture in availableCultures)
                    {
                        subProperty.SetValue(rawPropertyData.Value, availableCulture, segment);
                        if (published)
                        {
                            subProperty.PublishValues(availableCulture, segment ?? "*");
                        }
                        indexValues =
                            editor.PropertyIndexValueFactory.GetIndexValues(subProperty, availableCulture, segment, published, availableCultures, contentTypeDictionary);
                    }
                }
                else
                {
                    subProperty.SetValue(rawPropertyData.Value, propertyCulture, segment);
                    if (published)
                    {
                        subProperty.PublishValues(propertyCulture ?? "*", segment ?? "*");
                    }
                    indexValues = editor.PropertyIndexValueFactory.GetIndexValues(subProperty, propertyCulture, segment, published, availableCultures, contentTypeDictionary);
                }

                var rawDataCultures = rawData.Properties.Select(property => property.Culture).Distinct().WhereNotNull().ToArray();
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
            })
        };

    protected class RawDataItem
    {
        public required Guid ContentTypeKey { get; init; }

        public required IEnumerable<RawPropertyData> Properties { get; init; }
    }

    protected class RawPropertyData
    {
        public required string Alias { get; init; }

        public required object? Value { get; init; }

        public required string? Culture { get; init; }
    }
}
