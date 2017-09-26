using System;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <inheritdoc />
    /// <summary>
    /// A published property base that uses a raw object value.
    /// </summary>
    /// <remarks>Conversions results are stored within the property and will not
    /// be refreshed, so this class is not suitable for cached properties.</remarks>
    internal class RawValueProperty : PublishedPropertyBase
    {
        private readonly object _propertyData; //the value in the db
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;

        public override object SourceValue => _propertyData;

        public override bool HasValue => _propertyData is string s ? !string.IsNullOrWhiteSpace(s) : _propertyData != null;

        public override object Value => _objectValue.Value;

        public override object XPathValue => _xpathValue.Value;

        public RawValueProperty(PublishedPropertyType propertyType, IPublishedElement content, object propertyData, bool isPreviewing = false)
            : base(propertyType, PropertyCacheLevel.Unknown) // cache level is ignored
        {
            _propertyData = propertyData;

            var interValue = new Lazy<object>(() => PropertyType.ConvertSourceToInter(content, _propertyData, isPreviewing));
            _objectValue = new Lazy<object>(() => PropertyType.ConvertInterToObject(content, PropertyCacheLevel.Unknown, interValue.Value, isPreviewing));
            _xpathValue = new Lazy<object>(() => PropertyType.ConvertInterToXPath(content, PropertyCacheLevel.Unknown, interValue.Value, isPreviewing));
        }
    }
}
