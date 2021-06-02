using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache
{
    // notes:
    // a published element does NOT manage any tree-like elements, neither the
    // original NestedContent (from Lee) nor the DetachedPublishedContent POC did.
    //
    // at the moment we do NOT support models for sets - that would require
    // an entirely new models factory + not even sure it makes sense at all since
    // sets are created manually todo yes it does! - what does this all mean?
    //
    internal class PublishedElement : IPublishedElement
    {
        // initializes a new instance of the PublishedElement class
        // within the context of a published snapshot service (eg a published content property value)
        public PublishedElement(IPublishedContentType contentType, Guid key, Dictionary<string, object> values, bool previewing,
            PropertyCacheLevel referenceCacheLevel, IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            if (key == Guid.Empty) throw new ArgumentException("Empty guid.");
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (referenceCacheLevel != PropertyCacheLevel.None && publishedSnapshotAccessor == null)
                throw new ArgumentNullException("A published snapshot accessor is required when referenceCacheLevel != None.", nameof(publishedSnapshotAccessor));

            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            Key = key;

            values = GetCaseInsensitiveValueDictionary(values);

            _propertiesArray = contentType
                .PropertyTypes
                .Select(propertyType =>
                {
                    values.TryGetValue(propertyType.Alias, out var value);
                    return (IPublishedProperty) new PublishedElementPropertyBase(propertyType, this, previewing, referenceCacheLevel, value, publishedSnapshotAccessor);
                })
                .ToArray();
        }

        // initializes a new instance of the PublishedElement class
        // without any context, so it's purely 'standalone' and should NOT interfere with the published snapshot service
        // + using an initial reference cache level of .None ensures that everything will be
        // cached at .Content level - and that reference cache level will propagate to all
        // properties
        public PublishedElement(IPublishedContentType contentType, Guid key, Dictionary<string, object> values, bool previewing)
            : this(contentType, key, values, previewing, PropertyCacheLevel.None, null)
        { }

        private static Dictionary<string, object> GetCaseInsensitiveValueDictionary(Dictionary<string, object> values)
        {
            // ensure we ignore case for property aliases
            var comparer = values.Comparer;
            var ignoreCase = Equals(comparer, StringComparer.OrdinalIgnoreCase) || Equals(comparer, StringComparer.InvariantCultureIgnoreCase) || Equals(comparer, StringComparer.CurrentCultureIgnoreCase);
            return ignoreCase ? values :  new Dictionary<string, object>(values, StringComparer.OrdinalIgnoreCase);
        }

        #region ContentType

        public IPublishedContentType ContentType { get; }

        #endregion

        #region PublishedElement

        public Guid Key { get; }

        #endregion

        #region Properties

        private readonly IPublishedProperty[] _propertiesArray;

        public IEnumerable<IPublishedProperty> Properties => _propertiesArray;

        public IPublishedProperty GetProperty(string alias)
        {
            var index = ContentType.GetPropertyIndex(alias);
            var property = index < 0 ? null : _propertiesArray[index];
            return property;
        }

        #endregion
    }
}
