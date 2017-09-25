using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.Models
{
    internal class DetachedPublishedProperty : IPublishedProperty
    {
        private readonly PublishedPropertyType _propertyType;
        private readonly object _sourceValue;
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;
        private readonly bool _isPreview;

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value)
            : this(propertyType, value, false)
        { }

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value, bool isPreview)
        {
            _propertyType = propertyType;
            _isPreview = isPreview;

            _sourceValue = value;

            IPublishedElement publishedElement = null; // fixme!! nested content needs complete refactoring!

            var interValue = new Lazy<object>(() => _propertyType.ConvertSourceToInter(publishedElement, _sourceValue, _isPreview));
            _objectValue = new Lazy<object>(() => _propertyType.ConvertInterToObject(publishedElement, PropertyCacheLevel.None, interValue.Value, _isPreview));
            _xpathValue = new Lazy<object>(() => _propertyType.ConvertInterToXPath(publishedElement, PropertyCacheLevel.None, interValue.Value, _isPreview));
        }

        public string PropertyTypeAlias => _propertyType.PropertyTypeAlias;

        public bool HasValue => SourceValue != null && SourceValue.ToString().Trim().Length > 0;

        public object SourceValue => _sourceValue;

        public object Value => _objectValue.Value;

        public object XPathValue => _xpathValue.Value;
    }
}
