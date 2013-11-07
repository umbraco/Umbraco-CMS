using System;
using log4net.Core;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Indicates the cache level for a property cacheable value.
    /// </summary>
    /// <remarks>Use this attribute to mark property values converters.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class PropertyValueCacheAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueCacheAttribute"/> class with a cacheable value and a cache level.
        /// </summary>
        /// <param name="value">The cacheable value.</param>
        /// <param name="level">The cache level.</param>
        public PropertyValueCacheAttribute(PropertyCacheValue value, PropertyCacheLevel level)
        {
            Value = value;
            Level = level;
        }

        /// <summary>
        /// Gets or sets the cacheable value.
        /// </summary>
        public PropertyCacheValue Value { get; private set; }

        /// <summary>
        /// Gets or sets the cache level;
        /// </summary>
        public PropertyCacheLevel Level { get; private set; }
    }
}