using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class SliderValueConverter : PropertyValueConverterBase
    {
        private readonly IDataTypeService _dataTypeService;

        public SliderValueConverter(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Slider);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => IsRangeDataType(propertyType.DataType.Id) ? typeof (Range<decimal>) : typeof (decimal);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            if (source == null)
                return null;

            if (IsRangeDataType(propertyType.DataType.Id))
            {
                var rangeRawValues = source.ToString().Split(Constants.CharArrays.Comma);
                var minimumAttempt = rangeRawValues[0].TryConvertTo<decimal>();
                var maximumAttempt = rangeRawValues[1].TryConvertTo<decimal>();

                if (minimumAttempt.Success && maximumAttempt.Success)
                {
                    return new Range<decimal> { Maximum = maximumAttempt.Result, Minimum = minimumAttempt.Result };
                }
            }

            var valueAttempt = source.ToString().TryConvertTo<decimal>();
            if (valueAttempt.Success)
                return valueAttempt.Result;

            // Something failed in the conversion of the strings to decimals
            return null;

        }

        /// <summary>
        /// Discovers if the slider is set to range mode.
        /// </summary>
        /// <param name="dataTypeId">
        /// The data type id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsRangeDataType(int dataTypeId)
        {
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher
            // TODO: this is cheap now, remove the caching

            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var dataType = _dataTypeService.GetDataType(id);
                var configuration = dataType.ConfigurationAs<SliderConfiguration>();
                return configuration.EnableRange;
            });
        }

        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}
