using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    class PublishedFragmentProperty : PublishedPropertyBase
    {
        private readonly object _dataValue;
        private readonly PublishedFragment _content;

        private readonly Lazy<object> _sourceValue;
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;

        public PublishedFragmentProperty(PublishedPropertyType propertyType, PublishedFragment content)
            : this(propertyType, content, null)
        { }

        public PublishedFragmentProperty(PublishedPropertyType propertyType, PublishedFragment content, object dataValue)
            : base(propertyType)
        {
            _dataValue = dataValue;
            _content = content;

            _sourceValue = new Lazy<object>(() => PropertyType.ConvertDataToSource(_dataValue, _content.IsPreviewing));
            _objectValue = new Lazy<object>(() => PropertyType.ConvertSourceToObject(_sourceValue.Value, _content.IsPreviewing));
            _xpathValue = new Lazy<object>(() => PropertyType.ConvertSourceToXPath(_sourceValue.Value, _content.IsPreviewing));
        }

        public override bool HasValue
        {
            get { return _dataValue != null && ((_dataValue is string) == false || string.IsNullOrWhiteSpace((string)_dataValue) == false); }
        }

        public override object DataValue
        {
            get { return _dataValue; }
        }

        public override object Value { get { return _objectValue.Value; } }
        public override object XPathValue { get { return _xpathValue.Value; } }
    }
}
