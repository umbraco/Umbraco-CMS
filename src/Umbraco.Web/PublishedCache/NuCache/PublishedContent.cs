using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class PublishedContent : PublishedContentBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly ContentNode _contentNode;
        // ReSharper disable once InconsistentNaming
        internal readonly ContentData _contentData; // internal for ContentNode cloning

        private readonly string _urlName;
        private IReadOnlyDictionary<string, PublishedCultureName> _cultureNames;

        #region Constructors

        public PublishedContent(ContentNode contentNode, ContentData contentData, IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            _contentNode = contentNode;
            _contentData = contentData;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;

            _urlName = _contentData.Name.ToUrlSegment();
            IsPreviewing = _contentData.Published == false;

            var properties = new List<IPublishedProperty>();
            foreach (var propertyType in _contentNode.ContentType.PropertyTypes)
            {
                if (contentData.Properties.TryGetValue(propertyType.Alias, out var pdatas))
                    properties.Add(new Property(propertyType, this, pdatas, _publishedSnapshotAccessor));
            }
            PropertiesArray = properties.ToArray();
        }

        private string GetProfileNameById(int id)
        {
            var cache = GetCurrentSnapshotCache();
            return cache == null
                ? GetProfileNameByIdNoCache(id)
                : (string)cache.GetCacheItem(CacheKeys.ProfileName(id), () => GetProfileNameByIdNoCache(id));
        }

        private static string GetProfileNameByIdNoCache(int id)
        {
#if DEBUG
            var userService = Current.Services?.UserService;
            if (userService == null) return "[null]"; // for tests
#else
            // we don't want each published content to hold a reference to the service
            // so where should they get the service from really? from the locator...
            var userService = Current.Services.UserService;
#endif
            var user = userService.GetProfileById(id);
            return user?.Name;
        }

        // (see ContentNode.CloneParent)
        public PublishedContent(ContentNode contentNode, PublishedContent origin)
        {
            _contentNode = contentNode;
            _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
            _contentData = origin._contentData;

            _urlName = origin._urlName;
            IsPreviewing = origin.IsPreviewing;

            // here is the main benefit: we do not re-create properties so if anything
            // is cached locally, we share the cache - which is fine - if anything depends
            // on the tree structure, it should not be cached locally to begin with
            PropertiesArray = origin.PropertiesArray;
        }

        // clone for previewing as draft a published content that is published and has no draft
        private PublishedContent(PublishedContent origin)
        {
            _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
            _contentNode = origin._contentNode;
            _contentData = origin._contentData;

            _urlName = origin._urlName;
            IsPreviewing = true;

            // clone properties so _isPreviewing is true
            PropertiesArray = origin.PropertiesArray.Select(x => (IPublishedProperty)new Property((Property)x, this)).ToArray();
        }

        #endregion

        #region Get Content/Media for Parent/Children

        // this is for tests purposes
        // args are: current published snapshot (may be null), previewing, content id - returns: content

        internal static Func<IPublishedShapshot, bool, int, IPublishedContent> GetContentByIdFunc { get; set; }
            = (publishedShapshot, previewing, id) => publishedShapshot.Content.GetById(previewing, id);

        internal static Func<IPublishedShapshot, bool, int, IPublishedContent> GetMediaByIdFunc { get; set; }
            = (publishedShapshot, previewing, id) => publishedShapshot.Media.GetById(previewing, id);

        private IPublishedContent GetContentById(bool previewing, int id)
        {
            return GetContentByIdFunc(_publishedSnapshotAccessor.PublishedSnapshot, previewing, id);
        }

        private IEnumerable<IPublishedContent> GetContentByIds(bool previewing, IEnumerable<int> ids)
        {
            var publishedSnapshot = _publishedSnapshotAccessor.PublishedSnapshot;

            // beware! the loop below CANNOT be converted to query such as:
            //return ids.Select(x => _getContentByIdFunc(publishedSnapshot, previewing, x)).Where(x => x != null);
            // because it would capture the published snapshot and cause all sorts of issues
            //
            // we WANT to get the actual current published snapshot each time we run

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var id in ids)
            {
                var content = GetContentByIdFunc(publishedSnapshot, previewing, id);
                if (content != null) yield return content;
            }
        }

        private IPublishedContent GetMediaById(bool previewing, int id)
        {
            return GetMediaByIdFunc(_publishedSnapshotAccessor.PublishedSnapshot, previewing, id);
        }

        private IEnumerable<IPublishedContent> GetMediaByIds(bool previewing, IEnumerable<int> ids)
        {
            var publishedShapshot = _publishedSnapshotAccessor.PublishedSnapshot;

            // see note above for content

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var id in ids)
            {
                var content = GetMediaByIdFunc(publishedShapshot, previewing, id);
                if (content != null) yield return content;
            }
        }

        #endregion

        #region IPublishedContent

        public override int Id => _contentNode.Id;
        public override Guid Key => _contentNode.Uid;
        public override int DocumentTypeId => _contentNode.ContentType.Id;
        public override string DocumentTypeAlias => _contentNode.ContentType.Alias;
        public override PublishedItemType ItemType => _contentNode.ContentType.ItemType;

        public override string Name => _contentData.Name;
        public override IReadOnlyDictionary<string, PublishedCultureName> CultureNames
        {
            get
            {
                if (!ContentType.Variations.HasFlag(ContentVariation.CultureNeutral))
                    return null;

                if (_cultureNames == null)
                {
                    var d = new Dictionary<string, PublishedCultureName>();
                    foreach(var c in _contentData.CultureInfos)
                    {
                        d[c.Key] = new PublishedCultureName(c.Value.Name, c.Value.Name.ToUrlSegment());
                    }
                    _cultureNames = d;
                }
                return _cultureNames;
            }
        }
        public override int Level => _contentNode.Level;
        public override string Path => _contentNode.Path;
        public override int SortOrder => _contentNode.SortOrder;
        public override int TemplateId => _contentData.TemplateId;

        public override string UrlName => _urlName;

        public override DateTime CreateDate => _contentNode.CreateDate;
        public override DateTime UpdateDate => _contentData.VersionDate;

        public override int CreatorId => _contentNode.CreatorId;
        public override string CreatorName => GetProfileNameById(_contentNode.CreatorId);
        public override int WriterId => _contentData.WriterId;
        public override string WriterName => GetProfileNameById(_contentData.WriterId);

        public override bool IsDraft => _contentData.Published == false;

        public override IPublishedContent Parent
        {
            get
            {
                // have to use the "current" cache because a PublishedContent can be shared
                // amongst many snapshots and other content depend on the snapshots
                switch (_contentNode.ContentType.ItemType)
                {
                    case PublishedItemType.Content:
                        return GetContentById(IsPreviewing, _contentNode.ParentContentId);
                    case PublishedItemType.Media:
                        return GetMediaById(IsPreviewing, _contentNode.ParentContentId);
                    default:
                        throw new Exception($"Panic: unsupported item type \"{_contentNode.ContentType.ItemType}\".");
                }
            }
        }

        private string _childrenCacheKey;

        private string ChildrenCacheKey => _childrenCacheKey ?? (_childrenCacheKey = CacheKeys.PublishedContentChildren(Key, IsPreviewing));

        public override IEnumerable<IPublishedContent> Children
        {
            get
            {
                var cache = GetAppropriateCache();
                if (cache == null || PublishedSnapshotService.CachePublishedContentChildren == false)
                    return GetChildren();

                // note: ToArray is important here, we want to cache the result, not the function!
                return (IEnumerable<IPublishedContent>)cache.GetCacheItem(ChildrenCacheKey, () => GetChildren().ToArray());
            }
        }

        private IEnumerable<IPublishedContent> GetChildren()
        {
            IEnumerable<IPublishedContent> c;
            switch (_contentNode.ContentType.ItemType)
            {
                case PublishedItemType.Content:
                    c = GetContentByIds(IsPreviewing, _contentNode.ChildContentIds);
                    break;
                case PublishedItemType.Media:
                    c = GetMediaByIds(IsPreviewing, _contentNode.ChildContentIds);
                    break;
                default:
                    throw new Exception("oops");
            }

            return c.OrderBy(x => x.SortOrder);

            // notes:
            // _contentNode.ChildContentIds is an unordered int[]
            // need needs to fetch & sort - do it only once, lazyily, though
            // Q: perfs-wise, is it better than having the store managed an ordered list
        }

        public override IEnumerable<IPublishedProperty> Properties => PropertiesArray;

        public override IPublishedProperty GetProperty(string alias)
        {
            var index = _contentNode.ContentType.GetPropertyIndex(alias);
            if (index < 0) return null;
            //TODO: Should we log here? I think this can happen when property types are added/removed from the doc type and the json serialized properties
            // no longer match the list of property types since that is how the PropertiesArray is populated.
            //TODO: Does the PropertiesArray get repopulated on content save?
            if (index > PropertiesArray.Length) return null; 
            var property = PropertiesArray[index];
            return property;
        }

        public override IPublishedProperty GetProperty(string alias, bool recurse)
        {
            var property = GetProperty(alias);
            if (recurse == false) return property;

            var cache = GetAppropriateCache();
            if (cache == null)
                return base.GetProperty(alias, true);

            var key = ((Property)property).RecurseCacheKey;
            return (Property)cache.GetCacheItem(key, () => base.GetProperty(alias, true));
        }

        public override PublishedContentType ContentType => _contentNode.ContentType;

        #endregion

        #region Caching

        // beware what you use that one for - you don't want to cache its result
        private ICacheProvider GetAppropriateCache()
        {
            var publishedSnapshot = (PublishedShapshot)_publishedSnapshotAccessor.PublishedSnapshot;
            var cache = publishedSnapshot == null
                ? null
                : ((IsPreviewing == false || PublishedSnapshotService.FullCacheWhenPreviewing) && (ItemType != PublishedItemType.Member)
                    ? publishedSnapshot.ElementsCache
                    : publishedSnapshot.SnapshotCache);
            return cache;
        }

        private ICacheProvider GetCurrentSnapshotCache()
        {
            var publishedSnapshot = (PublishedShapshot)_publishedSnapshotAccessor.PublishedSnapshot;
            return publishedSnapshot?.SnapshotCache;
        }

        #endregion

        #region Internal

        // used by navigable content
        internal IPublishedProperty[] PropertiesArray { get; }

        // used by navigable content
        internal int ParentId => _contentNode.ParentContentId;

        // used by navigable content
        // includes all children, published or unpublished
        // NavigableNavigator takes care of selecting those it wants
        internal IList<int> ChildIds => _contentNode.ChildContentIds;

        // used by Property
        // gets a value indicating whether the content or media exists in
        // a previewing context or not, ie whether its Parent, Children, and
        // properties should refer to published, or draft content
        internal bool IsPreviewing { get; }

        private string _asPreviewingCacheKey;

        private string AsPreviewingCacheKey => _asPreviewingCacheKey ?? (_asPreviewingCacheKey = CacheKeys.PublishedContentAsPreviewing(Key));

        // used by ContentCache
        internal IPublishedContent AsPreviewingModel()
        {
            if (IsPreviewing)
                return this;

            var cache = GetAppropriateCache();
            if (cache == null) return new PublishedContent(this).CreateModel();
            return (IPublishedContent)cache.GetCacheItem(AsPreviewingCacheKey, () => new PublishedContent(this).CreateModel());
        }

        // used by Navigable.Source,...
        internal static PublishedContent UnwrapIPublishedContent(IPublishedContent content)
        {
            PublishedContentWrapped wrapped;
            while ((wrapped = content as PublishedContentWrapped) != null)
                content = wrapped.Unwrap();
            var inner = content as PublishedContent;
            if (inner == null)
                throw new InvalidOperationException("Innermost content is not PublishedContent.");
            return inner;
        }

        #endregion
    }
}
