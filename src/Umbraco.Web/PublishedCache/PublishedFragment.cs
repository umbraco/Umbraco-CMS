using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache
{
    // fixme document & review
    //
    // these things should NOT manage any tree-like elements (fixme:discuss?)
    // Lee's NestedContent package DetachedPublishedContent does NOT
    // and yet we need to be able to figure out what happens when nesting things?
    //
    // how would we create MODELS for published items? should it be automatic
    // or explicit when creating the items? probably cannot be automatic because
    // then, how would we cast the returned object?
    //
    // note: could also have a totally detached one in Core?
    //
    internal class PublishedFragment : IPublishedFragment
    {
        private readonly IFacadeAccessor _facadeAccessor;

        public PublishedFragment(PublishedContentType contentType,
            IFacadeAccessor facadeAccessor, PropertyCacheLevel referenceCacheLevel,
            Guid key, Dictionary<string, object> values,
            bool previewing)
        {
            ContentType = contentType;
            _facadeAccessor = facadeAccessor;
            Key = key;

            // ensure we ignore case for property aliases
            var comparer = values.Comparer;
            var ignoreCase = comparer == StringComparer.OrdinalIgnoreCase || comparer == StringComparer.InvariantCultureIgnoreCase || comparer == StringComparer.CurrentCultureIgnoreCase;
            if (ignoreCase == false)
                values = new Dictionary<string, object>(values, StringComparer.OrdinalIgnoreCase);

            _propertiesArray = contentType
                .PropertyTypes
                .Select(propertyType =>
                {
                    object value;
                    values.TryGetValue(propertyType.PropertyTypeAlias, out value);
                    return _facadeAccessor.Facade.CreateFragmentProperty(propertyType, Key, previewing, referenceCacheLevel, value);
                })
                .ToArray();

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
