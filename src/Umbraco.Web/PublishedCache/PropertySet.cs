using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache
{
    // notes:
    // a property set does NOT manage any tree-like elements, neither the
    // original NestedContent (from Lee) nor the DetachedPublishedContent POC did.
    //
    // at the moment we do NOT support models for sets - that would require
    // an entirely new models factory + not even sure it makes sense at all since
    // sets are created manually
    //
    internal class PropertySet : IPropertySet
    {
        // initializes a new instance of the PropertySet class
        // within the context of a facade service (eg a published content property value)
        public PropertySet(PublishedContentType contentType, Guid key, Dictionary<string, object> values, bool previewing,
            IFacadeService facadeService, PropertyCacheLevel referenceCacheLevel)
        {
            ContentType = contentType;
            Key = key;

            values = GetCaseInsensitiveValueDictionary(values);

            _propertiesArray = contentType
                .PropertyTypes
                .Select(propertyType =>
                {
                    values.TryGetValue(propertyType.PropertyTypeAlias, out object value);
                    return facadeService.CreateSetProperty(propertyType, this, previewing, referenceCacheLevel, value);
                })
                .ToArray();
        }

        // initializes a new instance of the PropertySet class
        // without any context, so it's purely 'standalone' and should NOT interfere with the facade service
        public PropertySet(PublishedContentType contentType, Guid key, Dictionary<string, object> values, bool previewing)
        {
            ContentType = contentType;
            Key = key;

            values = GetCaseInsensitiveValueDictionary(values);

            // using an initial reference cache level of .None ensures that everything will be
            // cached at .Content level - and that reference cache level will propagate to all
            // properties
            const PropertyCacheLevel cacheLevel = PropertyCacheLevel.None;

            _propertiesArray = contentType
                .PropertyTypes
                .Select(propertyType =>
                {
                    values.TryGetValue(propertyType.PropertyTypeAlias, out object value);
                    return (IPublishedProperty) new PropertySetProperty(propertyType, this, previewing, cacheLevel, value);
                })
                .ToArray();
        }

        private static Dictionary<string, object> GetCaseInsensitiveValueDictionary(Dictionary<string, object> values)
        {
            // ensure we ignore case for property aliases
            var comparer = values.Comparer;
            var ignoreCase = Equals(comparer, StringComparer.OrdinalIgnoreCase) || Equals(comparer, StringComparer.InvariantCultureIgnoreCase) || Equals(comparer, StringComparer.CurrentCultureIgnoreCase);
            return ignoreCase ? values :  new Dictionary<string, object>(values, StringComparer.OrdinalIgnoreCase);
        }

        #region ContentType

        public PublishedContentType ContentType { get; }

        #endregion

        #region PropertySet

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
