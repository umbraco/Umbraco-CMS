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
        private readonly Lazy<object> _sourceValue;
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;
        private readonly bool _isPreviewing;

        public override object SourceValue => _dbVal;

        public override bool HasValue => _dbVal != null && _dbVal.ToString().Trim().Length > 0;

        public override object Value => _objectValue.Value;

        public override object XPathValue => _xpathValue.Value;

        // note: propertyData cannot be null
        public RawValueProperty(PublishedPropertyType propertyType, object propertyData, bool isPreviewing = false)
            : this(propertyType, isPreviewing)
        {
            if (propertyData == null)
                throw new ArgumentNullException(nameof(propertyData));
            _dbVal = propertyData;
        }

        // note: maintaining two ctors to make sure we understand what we do when calling them
        public RawValueProperty(PublishedPropertyType propertyType, bool isPreviewing = false)
            : base(propertyType, PropertyCacheLevel.Unknown) // cache level is ignored
        {
            _dbVal = null;
            _isPreviewing = isPreviewing;

            _sourceValue = new Lazy<object>(() => PropertyType.ConvertSourceToInter(_dbVal, _isPreviewing));
            _objectValue = new Lazy<object>(() => PropertyType.ConvertInterToObject(PropertyCacheLevel.Unknown, _sourceValue.Value, _isPreviewing));
            _xpathValue = new Lazy<object>(() => PropertyType.ConvertInterToXPath(PropertyCacheLevel.Unknown, _sourceValue.Value, _isPreviewing));
        }
    }
}