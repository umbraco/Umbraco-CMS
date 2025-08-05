using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <inheritdoc />
public class DateTimeWithTimeZonePropertyIndexValueFactory : IDateTimeWithTimeZonePropertyIndexValueFactory
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    public DateTimeWithTimeZonePropertyIndexValueFactory(IDataTypeConfigurationCache dataTypeConfigurationCache)
        => _dataTypeConfigurationCache = dataTypeConfigurationCache;

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

        DateWithTimeZoneConfiguration? configuration = _dataTypeConfigurationCache.GetConfigurationAs<DateWithTimeZoneConfiguration>(property.PropertyType.DataTypeKey);
        var value = DateTimeWithTimeZoneValueConverter.GetValue(sourceStr, configuration);
        if (value is DateTimeOffset dateTimeOffset)
        {
            // Index the DateTimeOffset as UTC, so it's easier to query.
            value = dateTimeOffset.UtcDateTime;
        }

        indexValue.Values = [value];

        return [indexValue];
    }
}
