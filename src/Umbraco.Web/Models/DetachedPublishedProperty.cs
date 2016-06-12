using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
    internal class DetachedPublishedProperty : IPublishedProperty
    {
        private readonly PublishedPropertyType propertyType;
        private readonly object rawValue;
        private readonly Lazy<object> sourceValue;
        private readonly Lazy<object> objectValue;
        private readonly Lazy<object> xpathValue;
        private readonly bool isPreview;

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value)
            : this(propertyType, value, false)
        {
        }

        public DetachedPublishedProperty(PublishedPropertyType propertyType, object value, bool isPreview)
        {
            this.propertyType = propertyType;
            this.isPreview = isPreview;

            this.rawValue = value;

            this.sourceValue = new Lazy<object>(() => this.propertyType.ConvertDataToSource(this.rawValue, this.isPreview));
            this.objectValue = new Lazy<object>(() => this.propertyType.ConvertSourceToObject(this.sourceValue.Value, this.isPreview));
            this.xpathValue = new Lazy<object>(() => this.propertyType.ConvertSourceToXPath(this.sourceValue.Value, this.isPreview));
        }

        public string PropertyTypeAlias
        {
            get
            {
                return this.propertyType.PropertyTypeAlias;
            }
        }

        public bool HasValue
        {
            get { return DataValue != null && DataValue.ToString().Trim().Length > 0; }
        }

        public object DataValue { get { return this.rawValue; } }

        public object Value { get { return this.objectValue.Value; } }

        public object XPathValue { get { return this.xpathValue.Value; } }
    }
}
