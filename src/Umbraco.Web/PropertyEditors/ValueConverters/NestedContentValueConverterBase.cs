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
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        protected NestedContentValueConverterBase(IPublishedSnapshotAccessor publishedSnapshotAccessor, IPublishedModelFactory publishedModelFactory)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            PublishedModelFactory = publishedModelFactory;
        }

        protected IPublishedModelFactory PublishedModelFactory { get; }

        public static bool IsNested(PublishedPropertyType publishedProperty)
        {
            return publishedProperty.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.NestedContent);
        }

        public static bool IsNestedSingle(PublishedPropertyType publishedProperty)
        {
            if (!IsNested(publishedProperty))
                return false;

            var config = publishedProperty.DataType.ConfigurationAs<NestedContentConfiguration>();
            return config.MinItems == 1 && config.MaxItems == 1;
        }

        public static bool IsNestedMany(PublishedPropertyType publishedProperty)
        {
            return IsNested(publishedProperty) && !IsNestedSingle(publishedProperty);
        }

        protected IPublishedElement ConvertToElement(JObject sourceObject, PropertyCacheLevel referenceCacheLevel, bool preview)
        {
            var elementTypeAlias = sourceObject[NestedContentPropertyEditor.ContentTypeAliasPropertyKey]?.ToObject<string>();
            if (string.IsNullOrEmpty(elementTypeAlias))
                return null;

            // only convert element types - content types will cause an exception when PublishedModelFactory creates the model
            var publishedContentType = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetContentType(elementTypeAlias);
            if (publishedContentType == null || publishedContentType.IsElement == false)
                return null;

            var propertyValues = sourceObject.ToObject<Dictionary<string, object>>();

            if (!propertyValues.TryGetValue("key", out var keyo)
                || !Guid.TryParse(keyo.ToString(), out var key))
                key = Guid.Empty;

            IPublishedElement element = new PublishedElement(publishedContentType, key, propertyValues, preview, referenceCacheLevel, _publishedSnapshotAccessor);
            element = PublishedModelFactory.CreateModel(element);
            return element;
        }
    }
}
