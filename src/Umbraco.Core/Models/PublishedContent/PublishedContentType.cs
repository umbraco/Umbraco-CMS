﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly HashSet<string> _compositionAliases;

        // fast alias-to-index xref containing both the raw alias and its lowercase version
        private readonly Dictionary<string, int> _indexes = new Dictionary<string, int>();

        // internal so it can be used by PublishedNoCache which does _not_ want to cache anything and so will never
        // use the static cache getter PublishedContentType.GetPublishedContentType(alias) below - anything else
        // should use it.
        internal PublishedContentType(IContentTypeComposition contentType)
        {
            Id = contentType.Id;
            Alias = contentType.Alias;
            Name = contentType.Name;
            Description = contentType.Description;
            _compositionAliases = new HashSet<string>(contentType.CompositionAliases(), StringComparer.InvariantCultureIgnoreCase);
            _propertyTypes = contentType.CompositionPropertyTypes
                .Select(x => new PublishedPropertyType(this, x))
                .ToArray();
            InitializeIndexes();
        }

        // internal so it can be used for unit tests
        internal PublishedContentType(int id, string alias, string name, string description, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
        {
            Id = id;
            Alias = alias;
            Name = name;
            Description = description;
            _compositionAliases = new HashSet<string>(compositionAliases, StringComparer.InvariantCultureIgnoreCase);
            _propertyTypes = propertyTypes.ToArray();
            foreach (var propertyType in _propertyTypes)
                propertyType.ContentType = this;
            InitializeIndexes();
        }

        // create detached content type - ie does not match anything in the DB
        internal PublishedContentType(string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
            : this(0, alias, string.Empty, string.Empty, compositionAliases, propertyTypes)
        { }

        private void InitializeIndexes()
        {
            for (var i = 0; i < _propertyTypes.Length; i++)
            {
                var propertyType = _propertyTypes[i];
                _indexes[propertyType.PropertyTypeAlias] = i;
                _indexes[propertyType.PropertyTypeAlias.ToLowerInvariant()] = i;
            }
        }

        #region Content type

        public int Id { get; private set; }
        public string Alias { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public HashSet<string> CompositionAliases { get { return _compositionAliases; } }

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

        // these methods are called by ContentTypeCacheRefresher and DataTypeCacheRefresher

        internal static void ClearAll()
        {
            Logging.LogHelper.Debug<PublishedContentType>("Clear all.");
            // ok and faster to do it by types, assuming noone else caches PublishedContentType instances
            //ApplicationContext.Current.ApplicationCache.ClearStaticCacheByKeySearch("PublishedContentType_");
            ApplicationContext.Current.ApplicationCache.StaticCache.ClearCacheObjectTypes<PublishedContentType>();
        }

        internal static void ClearContentType(int id)
        {
            Logging.LogHelper.Debug<PublishedContentType>("Clear content type w/id {0}.", () => id);

            // we don't support "get all" at the moment - so, cheating
            var all = ApplicationContext.Current.ApplicationCache.StaticCache.GetCacheItemsByKeySearch<PublishedContentType>("PublishedContentType_").ToArray();

            // the one we want to clear
            var clr = all.FirstOrDefault(x => x.Id == id);
            if (clr == null) return;

            // those that have that one in their composition aliases
            // note: CompositionAliases contains all recursive aliases
            var oth = all.Where(x => x.CompositionAliases.InvariantContains(clr.Alias)).Select(x => x.Id);

            // merge ids
            var ids = oth.Concat(new[] { clr.Id }).ToArray();

            // clear them all at once
            // we don't support "clear many at once" at the moment - so, cheating
            ApplicationContext.Current.ApplicationCache.StaticCache.ClearCacheObjectTypes<PublishedContentType>(
                (key, value) => ids.Contains(value.Id));
        }

        internal static void ClearDataType(int id)
        {
            Logging.LogHelper.Debug<PublishedContentType>("Clear data type w/id {0}.", () => id);
            // there is no recursion to handle here because a PublishedContentType contains *all* its
            // properties ie both its own properties and those that were inherited (it's based upon an
            // IContentTypeComposition) and so every PublishedContentType having a property based upon
            // the cleared data type, be it local or inherited, will be cleared.
            ApplicationContext.Current.ApplicationCache.StaticCache.ClearCacheObjectTypes<PublishedContentType>(
                (key, value) => value.PropertyTypes.Any(x => x.DataTypeId == id));
        }

        public static PublishedContentType Get(PublishedItemType itemType, string alias)
        {
            var key = string.Format("PublishedContentType_{0}_{1}",
                itemType.ToString().ToLowerInvariant(), alias.ToLowerInvariant());

            var type = ApplicationContext.Current.ApplicationCache.StaticCache.GetCacheItem<PublishedContentType>(key,
                () => CreatePublishedContentType(itemType, alias));

            return type;
        }

        private static PublishedContentType CreatePublishedContentType(PublishedItemType itemType, string alias)
        {
            if (GetPublishedContentTypeCallback != null)
                return GetPublishedContentTypeCallback(alias);

            IContentTypeComposition contentType;
            switch (itemType)
            {
                case PublishedItemType.Content:
                    contentType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(alias);
                    break;
                case PublishedItemType.Media:
                    contentType = ApplicationContext.Current.Services.ContentTypeService.GetMediaType(alias);
                    break;
                case PublishedItemType.Member:
                    contentType = ApplicationContext.Current.Services.MemberTypeService.Get(alias);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("itemType");
            }

            if (contentType == null)
                throw new InvalidOperationException(string.Format("ContentTypeService failed to find a {0} type with alias \"{1}\".",
                    itemType.ToString().ToLower(), alias));

            return new PublishedContentType(contentType);
        }

        // for unit tests - changing the callback must reset the cache obviously
        private static Func<string, PublishedContentType> _getPublishedContentTypeCallBack;
        internal static Func<string, PublishedContentType> GetPublishedContentTypeCallback
        {
            get { return _getPublishedContentTypeCallBack; }
            set
            {
                // see note above
                //ClearAll();
                ApplicationContext.Current.ApplicationCache.StaticCache.ClearCacheByKeySearch("PublishedContentType_");

                _getPublishedContentTypeCallBack = value;
            }
        }

        #endregion
    }
}
