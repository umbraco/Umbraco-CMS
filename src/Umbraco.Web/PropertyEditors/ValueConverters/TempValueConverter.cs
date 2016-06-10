using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    // fixme
    // this is an experimental converter to work with nested etc.
    internal class TempValueConverter : PropertyValueConverterBase
    {
        private readonly IFacadeAccessor _facadeAccessor;

        public TempValueConverter(IFacadeAccessor facadeAccessor)
        {
            _facadeAccessor = facadeAccessor;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.ContentPickerAlias);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof (IPublishedFragment);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Snapshot;
        }

        public override object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview)
        {
            var json = source as string;
            return json == null ? null : JsonConvert.DeserializeObject<TempData>(json);
        }

        public override object ConvertInterToObject(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            var data = (TempData) inter;
            if (data == null) return null;

            var contentType = _facadeAccessor.Facade.ContentCache.GetContentType(data.ContentTypeAlias);
            if (contentType == null) return null;

            // fixme
            // note: if we wanted to returned a strongly-typed model here, we'd have to be explicit about it
            // so that we can tell GetPropertyValueType what we return - just relying on a factory is not
            // going to make it in any helpful way?

            return new PublishedFragment(contentType, _facadeAccessor, referenceCacheLevel, data.Key, data.Values, preview);
        }

        public override object ConvertInterToXPath(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            return inter == null ? null : JsonConvert.SerializeObject((TempData) inter);
        }

        public class TempData
        {
            public string ContentTypeAlias { get; set; }
            public Guid Key { get; set; }
            public Dictionary<string, object> Values { get; set; }
        }
    }
}
