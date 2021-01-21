using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class SliderValueConverter : PropertyValueConverterBase
    {
        public SliderValueConverter()
        { }

        [Obsolete("This constructor isn't required anymore, because we don't need services to lookup the data type configuration.")]
        public SliderValueConverter(IDataTypeService dataTypeService)
        { }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Slider);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => IsRangeDataType(propertyType) ? typeof(Range<decimal>) : typeof(decimal);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            var sourceString = source?.ToString();
            if (string.IsNullOrEmpty(sourceString))
            {
                return null;
            }

            if (IsRangeDataType(propertyType))
            {
                // Return range
                var rangeRawValues = sourceString.Split(',');
                var minimumAttempt = rangeRawValues[0].TryConvertTo<decimal>();

                if (rangeRawValues.Length == 1 && minimumAttempt.Success)
                {
                    // Configuration is probably changed from single to range, return range with same min/max
                    return new Range<decimal>
                    {
                        Minimum = minimumAttempt.Result,
                        Maximum = minimumAttempt.Result
                    };
                }
                else if (rangeRawValues.Length == 2)
                {
                    var maximumAttempt = rangeRawValues[1].TryConvertTo<decimal>();
                    if (maximumAttempt.Success)
                    {
                        return new Range<decimal>
                        {
                            Minimum = minimumAttempt.Result,
                            Maximum = maximumAttempt.Result
                        };
                    }
                }
            }
            else
            {
                // Return single value
                var valueAttempt = sourceString.TryConvertTo<decimal>();
                if (valueAttempt.Success)
                {
                    return valueAttempt.Result;
                }
            }

            // Something failed in the conversion of the strings to decimals
            return null;
        }

        private bool IsRangeDataType(IPublishedPropertyType propertyType)
            => propertyType.DataType.ConfigurationAs<SliderConfiguration>().EnableRange;
    }
}
