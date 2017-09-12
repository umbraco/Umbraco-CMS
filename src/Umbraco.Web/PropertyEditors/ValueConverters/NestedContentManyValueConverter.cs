using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    public class NestedContentManyValueConverter : PropertyValueConverterBase
    {
        private readonly ILogger _logger;

        public NestedContentManyValueConverter(ILogger logger)
        {
            _logger = logger;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.IsNestedContentProperty() && !propertyType.IsSingleNestedContentProperty();

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof(IEnumerable<IPublishedContent>);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Content;

        public override object ConvertSourceToInter(IPropertySet owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                return propertyType.ConvertPropertyToNestedContent(source, preview);
            }
            catch (Exception e)
            {
                _logger.Error<NestedContentManyValueConverter>("Error converting value", e);
            }

            return null;
        }
    }
}
