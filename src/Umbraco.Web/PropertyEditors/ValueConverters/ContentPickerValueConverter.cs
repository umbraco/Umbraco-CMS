using System;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    internal class ContentPickerValueConverter : PropertyValueConverterBase
    {
        private readonly IFacadeAccessor _facadeAccessor;

        public ContentPickerValueConverter(IFacadeAccessor facadeAccessor)
        {
            _facadeAccessor = facadeAccessor;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.ContentPickerAlias);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof (IPublishedContent);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Snapshot;
        }

        public override object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;

            int id;
            return int.TryParse(source.ToString(), out id) ? id : -1;
        }

        public override object ConvertInterToObject(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            var id = (int) inter;
            return id < 0 ? null : _facadeAccessor.Facade.ContentCache.GetById(id);
        }

        public override object ConvertInterToXPath(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            return inter.ToString();
        }
    }
}
