using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// A published property base that uses a raw object value.
    /// </summary>
    /// <remarks>Conversions results are stored within the property and will not
    /// be refreshed, so this class is not suitable for cached properties.</remarks>
    internal class RawValueProperty : PublishedPropertyBase
    {
        private readonly object _dbVal; //the value in the db
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;

        public override object SourceValue => _dbVal;

        public override bool HasValue => _dbVal != null && _dbVal.ToString().Trim().Length > 0;

        public override object Value => _objectValue.Value;

        public override object XPathValue => _xpathValue.Value;

        // note: propertyData cannot be null
        public RawValueProperty(PublishedPropertyType propertyType, IPublishedContent content, object propertyData, bool isPreviewing = false)
            : this(propertyType, content, isPreviewing)
        {
            _dbVal = propertyData ?? throw new ArgumentNullException(nameof(propertyData));
        }

        // note: maintaining two ctors to make sure we understand what we do when calling them
        public RawValueProperty(PublishedPropertyType propertyType, IPublishedContent content, bool isPreviewing = false)
            : base(propertyType, PropertyCacheLevel.Unknown) // cache level is ignored
        {
            _dbVal = null;
            var isPreviewing1 = isPreviewing;

            var sourceValue = new Lazy<object>(() => PropertyType.ConvertSourceToInter(content, _dbVal, isPreviewing1));
            _objectValue = new Lazy<object>(() => PropertyType.ConvertInterToObject(content, PropertyCacheLevel.Unknown, sourceValue.Value, isPreviewing1));
            _xpathValue = new Lazy<object>(() => PropertyType.ConvertInterToXPath(content, PropertyCacheLevel.Unknown, sourceValue.Value, isPreviewing1));
        }
    }
}
