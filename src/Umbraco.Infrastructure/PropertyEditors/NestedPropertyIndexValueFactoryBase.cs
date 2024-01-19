using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class NestedPropertyIndexValueFactoryBase<TSerialized, TItem> : JsonPropertyIndexValueFactoryBase<TSerialized>
{
    private readonly PropertyEditorCollection _propertyEditorCollection;


    protected NestedPropertyIndexValueFactoryBase(
        PropertyEditorCollection propertyEditorCollection,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(jsonSerializer, indexingSettings)
    {
        _propertyEditorCollection = propertyEditorCollection;
    }

    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 14.")]
    protected NestedPropertyIndexValueFactoryBase(
        PropertyEditorCollection propertyEditorCollection,
        IJsonSerializer jsonSerializer)
        : this(propertyEditorCollection, jsonSerializer, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {

    }

    [Obsolete("Use the overload that specifies availableCultures, scheduled for removal in v14")]
    protected override IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
        TSerialized deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published) =>
        Handle(deserializedPropertyValue, property, culture, segment, published, Enumerable.Empty<string>());

    [Obsolete("Use the overload that specifies availableCultures, scheduled for removal in v14")]
    protected override IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
        TSerialized deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures) =>
        Handle(
            deserializedPropertyValue,
            property,
            culture,
            segment,
            published,
            Enumerable.Empty<string>(),
            StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>().GetAll().ToDictionary(x=>x.Key));


    protected override IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
        TSerialized deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        var result = new List<KeyValuePair<string, IEnumerable<object?>>>();

        var index = 0;
        foreach (TItem nestedContentRowValue in GetDataItems(deserializedPropertyValue))
        {
            IContentType? contentType = GetContentTypeOfNestedItem(nestedContentRowValue, contentTypeDictionary);

            if (contentType is null)
            {
                continue;
            }

            var propertyTypeDictionary =
                contentType
                    .CompositionPropertyGroups
                    .SelectMany(x => x.PropertyTypes!)
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
                nestedContentRowValue,
                availableCultures));

            index++;
        }

        return RenameKeysToEnsureRawSegmentsIsAPrefix(result);
    }

    /// <summary>
    /// Rename keys that count the RAW-constant, to ensure the RAW-constant is a prefix.
    /// </summary>
    private IEnumerable<KeyValuePair<string, IEnumerable<object?>>> RenameKeysToEnsureRawSegmentsIsAPrefix(
        List<KeyValuePair<string, IEnumerable<object?>>> indexContent)
    {
        foreach (KeyValuePair<string, IEnumerable<object?>> indexedKeyValuePair in indexContent)
        {
            // Tests if key includes the RawFieldPrefix and it is not in the start
            if (indexedKeyValuePair.Key.Substring(1).Contains(UmbracoExamineFieldNames.RawFieldPrefix))
            {
                var newKey = UmbracoExamineFieldNames.RawFieldPrefix +
                             indexedKeyValuePair.Key.Replace(UmbracoExamineFieldNames.RawFieldPrefix, string.Empty);
                yield return new KeyValuePair<string, IEnumerable<object?>>(newKey, indexedKeyValuePair.Value);
            }
            else
            {
                yield return indexedKeyValuePair;
            }
        }
    }

    /// <summary>
    /// Gets the content type using the nested item.
    /// </summary>
    protected abstract IContentType? GetContentTypeOfNestedItem(TItem nestedItem, IDictionary<Guid, IContentType> contentTypeDictionary);

    [Obsolete("Use non-obsolete overload. Scheduled for removal in Umbraco 14.")]
    protected abstract IContentType? GetContentTypeOfNestedItem(TItem nestedItem);

    /// <summary>
    ///  Gets the raw data from a nested item.
    /// </summary>
    protected abstract IDictionary<string, object?> GetRawProperty(TItem nestedItem);

    /// <summary>
    /// Get the data times of a parent item. E.g. block list have contentData.
    /// </summary>
    protected abstract IEnumerable<TItem> GetDataItems(TSerialized input);

    /// <summary>
    /// Index a key with the name of the property, using the relevant content of all the children.
    /// </summary>
    protected override IEnumerable<KeyValuePair<string, IEnumerable<object?>>> HandleResume(
        List<KeyValuePair<string, IEnumerable<object?>>> indexedContent,
        IProperty property,
        string? culture,
        string? segment,
        bool published)
    {
        yield return new KeyValuePair<string, IEnumerable<object?>>(
            property.Alias,
            GetResumeFromAllContent(indexedContent).Yield());
    }

    /// <summary>
    /// Gets a resume as string of all the content in this nested type.
    /// </summary>
    /// <param name="indexedContent">All the indexed content for this property.</param>
    /// <returns>the string with all relevant content from </returns>
    private static string GetResumeFromAllContent(List<KeyValuePair<string, IEnumerable<object?>>> indexedContent)
    {
        var stringBuilder = new StringBuilder();
        foreach ((var indexKey, IEnumerable<object?>? indexedValue) in indexedContent)
        {
            // Ignore Raw fields
            if (indexKey.Contains(UmbracoExamineFieldNames.RawFieldPrefix))
            {
                continue;
            }

            foreach (var value in indexedValue)
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
    private IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetNestedResults(
        string keyPrefix,
        string? culture,
        string? segment,
        bool published,
        IDictionary<string, IPropertyType> propertyTypeDictionary,
        TItem nestedContentRowValue,
        IEnumerable<string> availableCultures)
    {
        foreach ((var propertyAlias, var propertyValue) in GetRawProperty(nestedContentRowValue))
        {
            if (propertyTypeDictionary.TryGetValue(propertyAlias, out IPropertyType? propertyType))
            {
                IDataEditor? editor = _propertyEditorCollection[propertyType.PropertyEditorAlias];
                if (editor is null)
                {
                    continue;
                }

                IProperty subProperty = new Property(propertyType);
                IEnumerable<KeyValuePair<string, IEnumerable<object?>>> indexValues = null!;

                if (propertyType.VariesByCulture() && culture is null)
                {
                    foreach (var availableCulture in availableCultures)
                    {
                        subProperty.SetValue(propertyValue, availableCulture, segment);
                        if (published)
                        {
                            subProperty.PublishValues(availableCulture, segment ?? "*");
                        }
                        indexValues =
                            editor.PropertyIndexValueFactory.GetIndexValues(subProperty, availableCulture, segment, published, availableCultures);
                    }
                }
                else
                {
                    subProperty.SetValue(propertyValue, culture, segment);
                    if (published)
                    {
                        subProperty.PublishValues(culture ?? "*", segment ?? "*");
                    }
                    indexValues = editor.PropertyIndexValueFactory.GetIndexValues(subProperty, culture, segment, published, availableCultures);
                }

                foreach ((var nestedAlias, IEnumerable<object?> nestedValue) in indexValues)
                {
                    yield return new KeyValuePair<string, IEnumerable<object?>>(
                        $"{keyPrefix}.{nestedAlias}", nestedValue!);
                }
            }
        }
    }
}
