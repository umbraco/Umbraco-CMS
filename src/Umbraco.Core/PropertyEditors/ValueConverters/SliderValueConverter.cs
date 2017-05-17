using System;
using System.Collections.Concurrent;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class SliderValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        private readonly IDataTypeService _dataTypeService;

        //TODO: Remove this ctor in v8 since the other one will use IoC
        public SliderValueConverter()
            : this(ApplicationContext.Current.Services.DataTypeService)
        {
        }

        public SliderValueConverter(IDataTypeService dataTypeService)
        {
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            _dataTypeService = dataTypeService;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
            {
                return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.SliderAlias);
            }
            return false;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            if (IsRangeDataType(propertyType.DataTypeId))
            {
                var rangeRawValues = source.ToString().Split(',');
                var minimumAttempt = rangeRawValues[0].TryConvertTo<decimal>();
                var maximumAttempt = rangeRawValues[1].TryConvertTo<decimal>();

                if (minimumAttempt.Success && maximumAttempt.Success)
                {
                    return new Range<decimal>() { Maximum = maximumAttempt.Result, Minimum = minimumAttempt.Result };
                }
            }

            var valueAttempt = source.ToString().TryConvertTo<decimal>();
            if (valueAttempt.Success)
            {
                return valueAttempt.Result;
            }

            // Something failed in the conversion of the strings to decimals
            return null;

        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            if (IsRangeDataType(propertyType.DataTypeId))
            {
                return typeof(Range<decimal>);
            }
            return typeof(decimal);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
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

            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var preValue = _dataTypeService.GetPreValuesCollectionByDataTypeId(id)
                    .PreValuesAsDictionary
                    .FirstOrDefault(x => string.Equals(x.Key, "enableRange", StringComparison.InvariantCultureIgnoreCase))
                    .Value;

                return preValue != null && preValue.Value.TryConvertTo<bool>().Result;
            });
        }

        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}
