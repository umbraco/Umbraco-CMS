using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    public class NestedContentSingleValueConverter : PropertyValueConverterBase
    {
        private readonly ILogger _logger;

        public NestedContentSingleValueConverter(ILogger logger)
        {
            _logger = logger;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.IsSingleNestedContentProperty();

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof(IPublishedContent);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Content;

        public override object ConvertSourceToInter(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                return propertyType.ConvertPropertyToNestedContent(source, preview);
            }
            catch (Exception e)
            {
                _logger.Error<NestedContentSingleValueConverter>("Error converting value", e);
            }

            return null;
        }
    }
}
