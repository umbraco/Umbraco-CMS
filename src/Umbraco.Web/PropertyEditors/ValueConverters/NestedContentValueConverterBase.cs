using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    public abstract class NestedContentValueConverterBase : PropertyValueConverterBase
    {
        private readonly IFacadeAccessor _facadeAccessor;
        private readonly Lazy<IFacadeService> _facadeService;

        protected NestedContentValueConverterBase(IFacadeAccessor facadeAccessor, Lazy<IFacadeService> facadeService, IPublishedModelFactory publishedModelFactory)
        {
            _facadeAccessor = facadeAccessor;
            _facadeService = facadeService;
            PublishedModelFactory = publishedModelFactory;
        }

        protected IFacadeService FacadeService => _facadeService.Value;

        protected IPublishedModelFactory PublishedModelFactory { get; }
    
        public static bool IsNested(PublishedPropertyType publishedProperty)
        {
            return publishedProperty.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.NestedContentAlias);
        }

        public static bool IsNestedSingle(PublishedPropertyType publishedProperty)
        {
            if (!IsNested(publishedProperty))
                return false;

            var preValueCollection = NestedContentHelper.GetPreValuesCollectionByDataTypeId(publishedProperty.DataTypeId);
            var preValueDictionary = preValueCollection.PreValuesAsDictionary;

            return preValueDictionary.TryGetValue("minItems", out var minItems)
                   && preValueDictionary.TryGetValue("maxItems", out var maxItems)
                   && int.TryParse(minItems.Value, out var minItemsValue) && minItemsValue == 1
                   && int.TryParse(maxItems.Value, out var maxItemsValue) && maxItemsValue == 1;
        }

        public static bool IsNestedMany(PublishedPropertyType publishedProperty)
        {
            return IsNested(publishedProperty) && !IsNestedSingle(publishedProperty);
        }

        protected IPublishedElement ConvertToElement(JObject sourceObject, PropertyCacheLevel referenceCacheLevel, bool preview)
        {
            var elementTypeAlias = NestedContentHelper.GetElementTypeAlias(sourceObject);
            if (string.IsNullOrEmpty(elementTypeAlias))
                return null;

            var publishedContentType = _facadeAccessor.Facade.ContentCache.GetContentType(elementTypeAlias);
            if (publishedContentType == null)
                return null;

            var propertyValues = sourceObject.ToObject<Dictionary<string, object>>();

            if (!propertyValues.TryGetValue("key", out var keyo)
                || !Guid.TryParse(keyo.ToString(), out var key))
                key = Guid.Empty;

            // fixme - why does it need a facade service here?
            IPublishedElement element = new PublishedElement(publishedContentType, key, propertyValues, preview, FacadeService, referenceCacheLevel);

            element = PublishedModelFactory.CreateModel(element);
            return element;
        }
    }
}
