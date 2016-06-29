using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache
{
    // notes:
    // a published fragment does NOT manage any tree-like elements, neither the
    // original NestedContent (from Lee) nor the DetachedPublishedContent POC did.
    //
    // at the moment we do NOT support models for fragments - that would require
    // an entirely new models factory + not even sure it makes sense at all since
    // fragments are created manually
    //
    internal class PublishedFragment : IPublishedFragment
    {
        // initializes a new instance of the PublishedFragment class
        // within the context of a facade service (eg a published content property value)
        public PublishedFragment(PublishedContentType contentType,
            IFacadeService facadeService, PropertyCacheLevel referenceCacheLevel,
            Guid key, Dictionary<string, object> values,
            bool previewing)
        {
            ContentType = contentType;
            Key = key;

            values = GetCaseInsensitiveValueDictionary(values);

            _propertiesArray = contentType
                .PropertyTypes
                .Select(propertyType =>
                {
                    object value;
                    values.TryGetValue(propertyType.PropertyTypeAlias, out value);
                    return facadeService.CreateFragmentProperty(propertyType, Key, previewing, referenceCacheLevel, value);
                })
                .ToArray();
        }

        // initializes a new instance of the PublishedFragment class
        // without any context, so it's purely 'standalone' and should NOT interfere with the facade service
        public PublishedFragment(PublishedContentType contentType, Guid key, Dictionary<string, object> values, bool previewing)
        {
            ContentType = contentType;
            Key = key;

            values = GetCaseInsensitiveValueDictionary(values);

            // using an initial reference cache level of .None ensures that
            // everything will be cached at .Content level
            // that reference cache level will propagate to all properties
            const PropertyCacheLevel cacheLevel = PropertyCacheLevel.None;

            _propertiesArray = contentType
                .PropertyTypes
                .Select(propertyType =>
                {
                    object value;
                    values.TryGetValue(propertyType.PropertyTypeAlias, out value);
                    return (IPublishedProperty) new PublishedFragmentProperty(propertyType, Key, previewing, cacheLevel, value);
                })
                .ToArray();
        }

        private static Dictionary<string, object> GetCaseInsensitiveValueDictionary(Dictionary<string, object> values)
        {
            // ensure we ignore case for property aliases
            var comparer = values.Comparer;
            var ignoreCase = comparer == StringComparer.OrdinalIgnoreCase || comparer == StringComparer.InvariantCultureIgnoreCase || comparer == StringComparer.CurrentCultureIgnoreCase;
            return ignoreCase ? values :  new Dictionary<string, object>(values, StringComparer.OrdinalIgnoreCase);
        }

        #region ContentType

        public PublishedContentType ContentType { get; }

        #endregion

        #region Content

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
