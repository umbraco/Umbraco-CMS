using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes tags property values as keywords, parsing either JSON or delimiter-separated storage.
/// </summary>
internal sealed class TagsPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize JSON-stored tag values.</param>
    /// <param name="dataTypeConfigurationCache">The cache used to resolve the property's tag configuration (storage type and delimiter).</param>
    public TagsPropertyValueHandler(IJsonSerializer jsonSerializer, IDataTypeConfigurationCache dataTypeConfigurationCache)
    {
        _jsonSerializer = jsonSerializer;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
    }

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Tags;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var values = ParsePropertyValue(property, culture, segment, published);
        return values?.Length > 0
            ? [new IndexField(property.Alias, new IndexValue { Keywords = values }, culture, segment)]
            : [];
    }

    private string[]? ParsePropertyValue(IProperty property, string? culture, string? segment, bool published)
    {
        var value = property.GetValue(culture, segment, published) as string;
        if (value.IsNullOrWhiteSpace())
        {
            return null;
        }

        TagConfiguration tagConfiguration = _dataTypeConfigurationCache.GetConfigurationAs<TagConfiguration>(property.PropertyType.DataTypeKey)
                                            ?? new TagConfiguration();
        tagConfiguration.Delimiter = tagConfiguration.Delimiter == default ? ',' : tagConfiguration.Delimiter;

        return tagConfiguration.StorageType switch
        {
            TagsStorageType.Json when value.DetectIsJson() => _jsonSerializer.Deserialize<string[]>(value),
            TagsStorageType.Csv => value.Split(tagConfiguration.Delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            _ => null
        };
    }
}
