using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using Umbraco.Core.Cache;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents an <see cref="IPublishedContent"/> type.
    /// </summary>
    /// <remarks>Instances of the <see cref="PublishedContentType"/> class are immutable, ie
    /// if the content type changes, then a new class needs to be created.</remarks>
    public class PublishedContentType
    {
        private readonly PublishedPropertyType[] _propertyTypes;

        // fast alias-to-index xref containing both the raw alias and its lowercase version
        private readonly Dictionary<string, int> _indexes = new Dictionary<string, int>();

        // internal so it can be used by PublishedNoCache which does _not_ want to cache anything and so will never
        // use the static cache getter PublishedContentType.GetPublishedContentType(alias) below - anything else
        // should use it.
        internal PublishedContentType(IContentTypeComposition contentType)
        {
            Id = contentType.Id;
            Alias = contentType.Alias;
            _propertyTypes = contentType.CompositionPropertyTypes
                .Select(x => new PublishedPropertyType(this, x))
                .ToArray();
            InitializeIndexes();
        }

        // internal so it can be used for unit tests
        internal PublishedContentType(int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes)
        {
            Id = id;
            Alias = alias;
            _propertyTypes = propertyTypes.ToArray();
            foreach (var propertyType in _propertyTypes)
                propertyType.ContentType = this;
            InitializeIndexes();
        }

        private void InitializeIndexes()
        {
            for (var i = 0; i < _propertyTypes.Length; i++)
            {
                var propertyType = _propertyTypes[i];
                _indexes[propertyType.Alias] = i;
                _indexes[propertyType.Alias.ToLowerInvariant()] = i;
            }
        }

        #region Content type

        public int Id { get; private set; }
        public string Alias { get; private set; }

        #endregion

        #region Properties

        public IEnumerable<PublishedPropertyType> PropertyTypes
        {
            get { return _propertyTypes; }
        }

        // alias is case-insensitive
        // this is the ONLY place where we compare ALIASES!
        public int GetPropertyIndex(string alias)
        {
            int index;
            if (_indexes.TryGetValue(alias, out index)) return index; // fastest
            if (_indexes.TryGetValue(alias.ToLowerInvariant(), out index)) return index; // slower
            return -1;
        }

        // virtual for unit tests
        public virtual PublishedPropertyType GetPropertyType(string alias)
        {
            var index = GetPropertyIndex(alias);
            return GetPropertyType(index);
        }

        // virtual for unit tests
        public virtual PublishedPropertyType GetPropertyType(int index)
        {
            return index >= 0 && index < _propertyTypes.Length ? _propertyTypes[index] : null;
        }

        #endregion

        #region Cache

        // note
        // default cache refresher events will contain the ID of the refreshed / removed IContentType
        // and not the alias. Also, we cannot hook into the cache refresher event here, because it belongs
        // to Umbraco.Web, so we do it in Umbraco.Web.Models.PublishedContentTypeCaching.
        
        // fixme
        // how do we know a content type has changed? if just the property has changed, do we trigger an event?
        // must run in debug mode to figure out... what happens when a DATATYPE changes? how do I get the type
        // of a content right with the content and be sure it's OK?
        // ******** HERE IS THE REAL ISSUE *******

        static readonly ConcurrentDictionary<string, PublishedContentType> ContentTypes = new ConcurrentDictionary<string, PublishedContentType>();
        
        // fixme - should not be public
        internal static void ClearAll()
        {
            Logging.LogHelper.Debug<PublishedContentType>("Clear all.");
            ContentTypes.Clear();
        }

        // fixme - should not be public
        internal static void ClearContentType(int id)
        {
            Logging.LogHelper.Debug<PublishedContentType>("Clear content type w/id {0}.", () => id);

            // see http://blogs.msdn.com/b/pfxteam/archive/2011/04/02/10149222.aspx
            // that should be race-cond safe
            ContentTypes.RemoveAll(kvp => kvp.Value.Id == id);
        }

        // fixme
        internal static void ClearDataType(int id)
        {
            Logging.LogHelper.Debug<PublishedContentType>("Clear data type w/id {0}.", () => id);

            // see note in ClearContentType()
            ContentTypes.RemoveAll(kvp => kvp.Value.PropertyTypes.Any(x => x.DataTypeId == id));
        }

        public static PublishedContentType Get(PublishedItemType itemType, string alias)
        {
            var key = (itemType == PublishedItemType.Content ? "content" : "media") + "::" + alias.ToLowerInvariant();
            return ContentTypes.GetOrAdd(key, k => CreatePublishedContentType(itemType, alias));
        }

        private static PublishedContentType CreatePublishedContentType(PublishedItemType itemType, string alias)
        {
            if (GetPublishedContentTypeCallback != null)
                return GetPublishedContentTypeCallback(alias);

            var contentType = itemType == PublishedItemType.Content
                ? (IContentTypeComposition) ApplicationContext.Current.Services.ContentTypeService.GetContentType(alias)
                : (IContentTypeComposition) ApplicationContext.Current.Services.ContentTypeService.GetMediaType(alias);

            return new PublishedContentType(contentType);
        }

        // for unit tests - changing the callback must reset the cache obviously
        private static Func<string, PublishedContentType> _getPublishedContentTypeCallBack;
        internal static Func<string, PublishedContentType> GetPublishedContentTypeCallback
        {
            get { return _getPublishedContentTypeCallBack; }
            set
            {
                ClearAll();
                _getPublishedContentTypeCallBack = value;
            }
        }

        #endregion
    }
}
