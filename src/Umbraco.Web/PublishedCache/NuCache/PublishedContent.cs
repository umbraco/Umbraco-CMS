using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class PublishedContent : PublishedContentBase
    {
        private readonly IFacadeAccessor _facadeAccessor;
        private readonly ContentNode _contentNode;
        // ReSharper disable once InconsistentNaming
        internal readonly ContentData _contentData; // internal for ContentNode cloning

        private readonly string _urlName;

        #region Constructors

        public PublishedContent(ContentNode contentNode, ContentData contentData, IFacadeAccessor facadeAccessor)
        {
            _contentNode = contentNode;
            _contentData = contentData;
            _facadeAccessor = facadeAccessor;

            _urlName = _contentData.Name.ToUrlSegment();
            IsPreviewing = _contentData.Published == false;

            var values = contentData.Properties;
            PropertiesArray = _contentNode.ContentType
                .PropertyTypes
                .Select(propertyType =>
                {
                    object value;
                    return values.TryGetValue(propertyType.PropertyTypeAlias, out value) && value != null
                        ? new Property(propertyType, this, value, _facadeAccessor) as IPublishedProperty
                        : new Property(propertyType, this, _facadeAccessor) as IPublishedProperty;
                })
                .ToArray();
        }

        private string GetProfileNameById(int id)
        {
            var cache = GetCurrentFacadeCache();
            return cache == null
                ? GetProfileNameByIdNoCache(id)
                : (string) cache.GetCacheItem(CacheKeys.ProfileName(id), () => GetProfileNameByIdNoCache(id));
        }

        private static string GetProfileNameByIdNoCache(int id)
        {
#if DEBUG
            var context = ApplicationContext.Current;
            var servicesContext = context?.Services;
            var userService = servicesContext?.UserService;
            if (userService == null) return "[null]"; // for tests
#else
            // we don't want each published content to hold a reference to the service
            // so where should they get the service from really? from the source...
            var userService = ApplicationContext.Current.Services.UserService;
#endif
            var user = userService.GetProfileById(id);
            return user?.Name;
        }

        // (see ContentNode.CloneParent)
        public PublishedContent(ContentNode contentNode, PublishedContent origin)
        {
            _contentNode = contentNode;
            _facadeAccessor = origin._facadeAccessor;
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
            _facadeAccessor = origin._facadeAccessor;
            _contentNode = origin._contentNode;
            _contentData = origin._contentData;

            _urlName = origin._urlName;
            IsPreviewing = true;

            // clone properties so _isPreviewing is true
            PropertiesArray = origin.PropertiesArray.Select(x => (IPublishedProperty) new Property((Property) x)).ToArray();
        }

        #endregion

        #region Get Content/Media for Parent/Children

        // this is for tests purposes
        // args are: current facade (may be null), previewing, content id - returns: content

        internal static Func<IFacade, bool, int, IPublishedContent> GetContentByIdFunc { get; set; }
            = (facade, previewing, id) => facade.ContentCache.GetById(previewing, id);

        internal static Func<IFacade, bool, int, IPublishedContent> GetMediaByIdFunc { get; set; }
            = (facade, previewing, id) => facade.MediaCache.GetById(previewing, id);

        private IPublishedContent GetContentById(bool previewing, int id)
        {
            return GetContentByIdFunc(_facadeAccessor.Facade, previewing, id);
        }

        private IEnumerable<IPublishedContent> GetContentByIds(bool previewing, IEnumerable<int> ids)
        {
            var facade = _facadeAccessor.Facade;

            // beware! the loop below CANNOT be converted to query such as:
            //return ids.Select(x => _getContentByIdFunc(facade, previewing, x)).Where(x => x != null);
            // because it would capture the facade and cause all sorts of issues
            //
            // we WANT to get the actual current facade each time we run

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var id in ids)
            {
                var content = GetContentByIdFunc(facade, previewing, id);
                if (content != null) yield return content;
            }
        }

        private IPublishedContent GetMediaById(bool previewing, int id)
        {
            return GetMediaByIdFunc(_facadeAccessor.Facade, previewing, id);
        }

        private IEnumerable<IPublishedContent> GetMediaByIds(bool previewing, IEnumerable<int> ids)
        {
            var facade = _facadeAccessor.Facade;

            // see note above for content

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var id in ids)
            {
                var content = GetMediaByIdFunc(facade, previewing, id);
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
        public override int Level => _contentNode.Level;
        public override string Path => _contentNode.Path;
        public override int SortOrder => _contentNode.SortOrder;
        public override Guid Version => _contentData.Version;
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
                        throw new Exception("oops");
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
                if (cache == null || FacadeService.CachePublishedContentChildren == false)
                    return GetChildren();

                // note: ToArray is important here, we want to cache the result, not the function!
                return (IEnumerable<IPublishedContent>) cache.GetCacheItem(ChildrenCacheKey, () => GetChildren().ToArray());
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
            var property = index < 0 ? null : PropertiesArray[index];
            return property;
        }

        public override IPublishedProperty GetProperty(string alias, bool recurse)
        {
            var property = GetProperty(alias);
            if (recurse == false) return property;

            var cache = GetAppropriateCache();
            if (cache == null)
                return base.GetProperty(alias, true);

            var key = ((Property) property).RecurseCacheKey;
            return (Property) cache.GetCacheItem(key, () => base.GetProperty(alias, true));
        }

        public override PublishedContentType ContentType => _contentNode.ContentType;

        #endregion

        #region Caching

        // beware what you use that one for - you don't want to cache its result
        private ICacheProvider GetAppropriateCache()
        {
            var facade = (Facade) _facadeAccessor.Facade;
            var cache = facade == null
                ? null
                : ((IsPreviewing == false || FacadeService.FullCacheWhenPreviewing) && (ItemType != PublishedItemType.Member)
                    ? facade.SnapshotCache
                    : facade.FacadeCache);
            return cache;
        }

        private ICacheProvider GetCurrentFacadeCache()
        {
            var facade = (Facade) _facadeAccessor.Facade;
            return facade?.FacadeCache;
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
            return (IPublishedContent) cache.GetCacheItem(AsPreviewingCacheKey, () => new PublishedContent(this).CreateModel());
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
