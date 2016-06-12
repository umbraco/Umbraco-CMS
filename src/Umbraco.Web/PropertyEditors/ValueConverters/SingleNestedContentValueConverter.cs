using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    [PropertyValueType(typeof(IPublishedContent))]
    public class SingleNestedContentValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return NestedContentHelper.IsSingleNestedContentProperty(propertyType);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                return NestedContentHelper.ConvertPropertyToNestedContent(propertyType, source).FirstOrDefault();
            }
            catch (Exception e)
            {
                LogHelper.Error<SingleNestedContentValueConverter>("Error converting value", e);
            }

            return null;
        }
    }
}
