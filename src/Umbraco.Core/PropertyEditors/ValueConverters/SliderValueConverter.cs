using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    public class SliderValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
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
            // ** This must be cached (U4-8862) **
            var dts = ApplicationContext.Current.Services.DataTypeService;
            var enableRange =
                dts.GetPreValuesCollectionByDataTypeId(dataTypeId)
                    .PreValuesAsDictionary.FirstOrDefault(
                        x => string.Equals(x.Key, "enableRange", StringComparison.InvariantCultureIgnoreCase)).Value;

            return enableRange != null && enableRange.Value.TryConvertTo<bool>().Result;
        }
    }
}
