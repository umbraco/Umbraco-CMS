using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
    internal class DetachedPublishedProperty : IPublishedProperty
    {
        private readonly PublishedPropertyType _propertyType;
        private readonly object _rawValue;
        private readonly Lazy<object> _sourceValue;
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;
        private readonly bool _isPreview;

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value)
            : this(propertyType, value, false)
        {
        }

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value, bool isPreview)
        {
            _propertyType = propertyType;
            _isPreview = isPreview;

            _rawValue = value;

            _sourceValue = new Lazy<object>(() => _propertyType.ConvertDataToSource(_rawValue, _isPreview));
            _objectValue = new Lazy<object>(() => _propertyType.ConvertSourceToObject(_sourceValue.Value, _isPreview));
            _xpathValue = new Lazy<object>(() => _propertyType.ConvertSourceToXPath(_sourceValue.Value, _isPreview));
        }

        public string PropertyTypeAlias
        {
            get
            {
                return _propertyType.PropertyTypeAlias;
            }
        }

        public bool HasValue
        {
            get { return DataValue != null && DataValue.ToString().Trim().Length > 0; }
        }

        public object DataValue { get { return _rawValue; } }

        public object Value { get { return _objectValue.Value; } }

        public object XPathValue { get { return _xpathValue.Value; } }
    }
}
