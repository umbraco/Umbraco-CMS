using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class TagsValueConverter : PropertyValueConverterBase
{
    private static readonly ConcurrentDictionary<int, bool> Storages = new();
    private readonly IDataTypeService _dataTypeService;
    private readonly IJsonSerializer _jsonSerializer;

    public TagsValueConverter(IDataTypeService dataTypeService, IJsonSerializer jsonSerializer)
    {
        _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
    }

    public static void ClearCaches() => Storages.Clear();

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Tags);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<string>);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return Array.Empty<string>();
        }

        // if Json storage type deserialize and return as string array
        if (JsonStorageType(propertyType.DataType.Id))
        {
            var array = source.ToString() is not null
                ? _jsonSerializer.Deserialize<string[]>(source.ToString()!)
                : null;
            return array ?? Array.Empty<string>();
        }

        // Otherwise assume CSV storage type and return as string array
        return source.ToString()?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
    }

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview) => (string[]?)source;

    /// <summary>
    ///     Discovers if the tags data type is storing its data in a Json format
    /// </summary>
    /// <param name="dataTypeId">
    ///     The data type id.
    /// </param>
    /// <returns>
    ///     The <see cref="bool" />.
    /// </returns>
    private bool JsonStorageType(int dataTypeId) =>

        // GetDataType(id) is cached at repository level; still, there is some
        // deep-cloning involved (expensive) - better cache here + trigger
        // refresh in DataTypeCacheRefresher
        Storages.GetOrAdd(dataTypeId, id =>
        {
            TagConfiguration? configuration = _dataTypeService.GetDataType(id)?.ConfigurationAs<TagConfiguration>();
            return configuration?.StorageType == TagsStorageType.Json;
        });
}
