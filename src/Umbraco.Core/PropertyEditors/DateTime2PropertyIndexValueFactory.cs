using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <inheritdoc />
public class DateTime2PropertyIndexValueFactory : IDateTime2PropertyIndexValueFactory
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;
    private readonly IJsonSerializer _jsonSerializer;

    public DateTime2PropertyIndexValueFactory(
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        IJsonSerializer jsonSerializer)
    {
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
        _jsonSerializer = jsonSerializer;
    }

    /// <inheritdoc />
    public IEnumerable<IndexValue> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        var indexValue = new IndexValue
        {
            Culture = culture,
            FieldName = property.Alias,
            Values = [],
        };

        var propertyValue = property.GetValue(culture, segment, published);
        var sourceStr = propertyValue?.ToString();
        if (sourceStr is null)
        {
            return [indexValue];
        }

        DateTime2Configuration? configuration = _dataTypeConfigurationCache.GetConfigurationAs<DateTime2Configuration>(property.PropertyType.DataTypeKey);
        var value = DateTime2ValueConverter.GetValue(sourceStr, configuration, _jsonSerializer);
        if (value is null)
        {
            return [indexValue];
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            // Index the DateTimeOffset as UTC, so it's easier to query.
            value = dateTimeOffset.UtcDateTime;
        }

        indexValue.Values = [$"{value:O}"];

        return [indexValue];
    }
}
