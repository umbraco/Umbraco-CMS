using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class SliderValueConverter : PropertyValueConverterBase
{
    private static readonly ConcurrentDictionary<int, bool> Storages = new();
    private readonly IDataTypeService _dataTypeService;

    public SliderValueConverter(IDataTypeService dataTypeService) => _dataTypeService =
        dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));

    public static void ClearCaches() => Storages.Clear();

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Slider);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => IsRangeDataType(propertyType.DataType.Id) ? typeof(Range<decimal>) : typeof(decimal);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        if (IsRangeDataType(propertyType.DataType.Id))
        {
            var rangeRawValues = source.ToString()!.Split(Constants.CharArrays.Comma);
            Attempt<decimal> minimumAttempt = rangeRawValues[0].TryConvertTo<decimal>();
            Attempt<decimal> maximumAttempt = rangeRawValues[1].TryConvertTo<decimal>();

            if (minimumAttempt.Success && maximumAttempt.Success)
            {
                return new Range<decimal> { Maximum = maximumAttempt.Result, Minimum = minimumAttempt.Result };
            }
        }

        Attempt<decimal> valueAttempt = source.ToString().TryConvertTo<decimal>();
        if (valueAttempt.Success)
        {
            return valueAttempt.Result;
        }

        // Something failed in the conversion of the strings to decimals
        return null;
    }

    /// <summary>
    ///     Discovers if the slider is set to range mode.
    /// </summary>
    /// <param name="dataTypeId">
    ///     The data type id.
    /// </param>
    /// <returns>
    ///     The <see cref="bool" />.
    /// </returns>
    private bool IsRangeDataType(int dataTypeId) =>

        // GetPreValuesCollectionByDataTypeId is cached at repository level;
        // still, the collection is deep-cloned so this is kinda expensive,
        // better to cache here + trigger refresh in DataTypeCacheRefresher
        // TODO: this is cheap now, remove the caching
        Storages.GetOrAdd(dataTypeId, id =>
        {
            IDataType? dataType = _dataTypeService.GetDataType(id);
            SliderConfiguration? configuration = dataType?.ConfigurationAs<SliderConfiguration>();
            return configuration?.EnableRange ?? false;
        });
}
