using System;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a base class for <c>IPublishedProperty</c> implementations which converts and caches
    /// the value source to the actual value to use when rendering content.
    /// </summary>
    internal abstract class PublishedPropertyBase : IPublishedProperty
    {
        public readonly PublishedPropertyType PropertyType;

        protected PublishedPropertyBase(PublishedPropertyType propertyType)
        {
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            PropertyType = propertyType;
        }

        public string PropertyTypeAlias
        {
            get { return PropertyType.PropertyTypeAlias; }
        }

        // these have to be provided by the actual implementation
        public abstract bool HasValue { get; }
        public abstract object DataValue { get; }
        public abstract object Value { get; }
        public abstract object XPathValue { get; }
    }
}
