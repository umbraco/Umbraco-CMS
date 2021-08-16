using System;
using System.Diagnostics;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a base class for <c>IPublishedProperty</c> implementations which converts and caches
    /// the value source to the actual value to use when rendering content.
    /// </summary>
    [DebuggerDisplay("{Alias} ({PropertyType?.EditorAlias})")]
    internal abstract class PublishedPropertyBase : IPublishedProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedPropertyBase"/> class.
        /// </summary>
        protected PublishedPropertyBase(IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel)
        {
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            ReferenceCacheLevel = referenceCacheLevel;

            ValidateCacheLevel(ReferenceCacheLevel, true);
            ValidateCacheLevel(PropertyType.CacheLevel, false);
        }

        // validates the cache level
        private static void ValidateCacheLevel(PropertyCacheLevel cacheLevel, bool validateUnknown)
        {
            switch (cacheLevel)
            {
                case PropertyCacheLevel.Element:
                case PropertyCacheLevel.Elements:
                case PropertyCacheLevel.Snapshot:
                case PropertyCacheLevel.None:
                    break;
                case PropertyCacheLevel.Unknown:
                    if (!validateUnknown) goto default;
                    break;
                default:
                    throw new Exception($"Invalid cache level \"{cacheLevel}\".");
            }
        }

        /// <summary>
        /// Gets the property type.
        /// </summary>
        public IPublishedPropertyType PropertyType { get; }

        /// <summary>
        /// Gets the property reference cache level.
        /// </summary>
        public PropertyCacheLevel ReferenceCacheLevel { get; }

        /// <inheritdoc />
        public string Alias => PropertyType.Alias;

        /// <inheritdoc />
        public abstract bool HasValue(string culture = null, string segment = null);

        /// <inheritdoc />
        public abstract object GetSourceValue(string culture = null, string segment = null);

        /// <inheritdoc />
        public abstract object GetValue(string culture = null, string segment = null);

        /// <inheritdoc />
        public abstract object GetXPathValue(string culture = null, string segment = null);
    }
}
