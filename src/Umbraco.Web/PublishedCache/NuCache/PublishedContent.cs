using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class PublishedContent : PublishedContentWithKeyBase
    {
        private readonly ContentNode _contentNode;
        // ReSharper disable once InconsistentNaming
        internal readonly ContentData _contentData; // internal for ContentNode cloning

        private readonly IPublishedProperty[] _properties;
        private readonly string _urlName;
        private readonly bool _isPreviewing;

        #region Constructors

        public PublishedContent(ContentNode contentNode, ContentData contentData)
        {
            _contentNode = contentNode;
            _contentData = contentData;

            _urlName = _contentData.Name.ToUrlSegment();
            _isPreviewing = _contentData.Published == false;
            _properties = CreateProperties(this, contentData.Properties);
        }

        private static string GetProfileNameById(int id)
        {
            var facade = Facade.Current;
            var cache = facade == null ? null : facade.FacadeCache;
            return cache == null
                ? GetProfileNameByIdNoCache(id)
                : (string) cache.GetCacheItem(CacheKeys.ProfileName(id), () => GetProfileNameByIdNoCache(id));
        }

        private static string GetProfileNameByIdNoCache(int id)
        {
#if DEBUG
            var context = ApplicationContext.Current;
            var servicesContext = context == null ? null : context.Services;
            var userService = servicesContext == null ? null : servicesContext.UserService;
            if (userService == null) return "[null]"; // for tests
#else
            // we don't want each published content to hold a reference to the service
            // so where should they get the service from really? from the source...
            var userService = ApplicationContext.Current.Services.UserService;
#endif
            var user = userService.GetProfileById(id);
            return user == null ? null : user.Name;
        }

        private static IPublishedProperty[] CreateProperties(PublishedContent content, IDictionary<string, object> values)
        {
            return content._contentNode.ContentType
                .PropertyTypes
                .Select(propertyType =>
                {
                    object value;
                    return values.TryGetValue(propertyType.PropertyTypeAlias, out value)
                        ? (IPublishedProperty) new Property(propertyType, content, value)
                        : (IPublishedProperty) new Property(propertyType, content);
                })
                .ToArray();
        }

        // (see ContentNode.CloneParent)
        public PublishedContent(ContentNode contentNode, PublishedContent origin)
        {
            _contentNode = contentNode;
            _contentData = origin._contentData;

            _urlName = origin._urlName;
            _isPreviewing = origin._isPreviewing;

            // here is the main benefit: we do not re-create properties so if anything
            // is cached locally, we share the cache - which is fine - if anything depends
            // on the tree structure, it should not be cached locally to begin with
            _properties = origin._properties;
        }

        // clone for previewing as draft a published content that is published and has no draft
        private PublishedContent(PublishedContent origin)
        {
            _contentNode = origin._contentNode;
            _contentData = origin._contentData;

            _urlName = origin._urlName;
            _isPreviewing = true;

            // clone properties so _isPreviewing is true
            _properties = origin._properties.Select(x => (IPublishedProperty) new Property((Property) x)).ToArray();
        }

        #endregion

        #region Get Content/Media for Parent/Children

        // this is for tests purposes
        // args are: current facade (may be null), previewing, content id - returns: content
        private static Func<IFacade, bool, int, IPublishedContent> _getContentByIdFunc =
            (facade, previewing, id) => facade.ContentCache.GetById(previewing, id);
        private static Func<IFacade, bool, int, IPublishedContent> _getMediaByIdFunc =
            (facade, previewing, id) => facade.MediaCache.GetById(previewing, id);

        internal static Func<IFacade, bool, int, IPublishedContent> GetContentByIdFunc
        {
            get { return _getContentByIdFunc; }
            set
            {
                _getContentByIdFunc = value;
            }
        }

        internal static Func<IFacade, bool, int, IPublishedContent> GetMediaByIdFunc
        {
            get { return _getMediaByIdFunc; }
            set
            {
                _getMediaByIdFunc = value;
            }
        }

        private static IPublishedContent GetContentById(bool previewing, int id)
        {
            return _getContentByIdFunc(Facade.Current, previewing, id);
        }

        private static IEnumerable<IPublishedContent> GetContentByIds(bool previewing, IEnumerable<int> ids)
        {
            var facade = Facade.Current;

            // beware! the loop below CANNOT be converted to query such as:
            //return ids.Select(x => _getContentByIdFunc(facade, previewing, x)).Where(x => x != null);
            // because it would capture the facade and cause all sorts of issues
            //
            // we WANT to get the actual current facade each time we run

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var id in ids)
            {
                var content = _getContentByIdFunc(facade, previewing, id);
                if (content != null) yield return content;
            }
        }

        private static IPublishedContent GetMediaById(bool previewing, int id)
        {
            return _getMediaByIdFunc(Facade.Current, previewing, id);
        }

        private static IEnumerable<IPublishedContent> GetMediaByIds(bool previewing, IEnumerable<int> ids)
        {
            var facade = Facade.Current;

            // see note above for content

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var id in ids)
            {
                var content = _getMediaByIdFunc(facade, previewing, id);
                if (content != null) yield return content;
            }
        }

        #endregion

        #region IPublishedContent

        public override int Id { get { return _contentNode.Id; } }
        public override Guid Key { get { return _contentNode.Uid; } }
        public override int DocumentTypeId { get { return _contentNode.ContentType.Id; } }
        public override string DocumentTypeAlias { get { return _contentNode.ContentType.Alias; } }
        public override PublishedItemType ItemType { get { return _contentNode.ContentType.ItemType; } }

        public override string Name { get { return _contentData.Name; } }
        public override int Level { get { return _contentNode.Level; } }
        public override string Path { get { return _contentNode.Path; } }
        public override int SortOrder { get { return _contentNode.SortOrder; } }
        public override Guid Version { get { return _contentData.Version; } }
        public override int TemplateId { get { return _contentData.TemplateId; } }

        public override string UrlName { get { return _urlName; } }

        public override DateTime CreateDate { get { return _contentNode.CreateDate; } }
        public override DateTime UpdateDate { get { return _contentData.VersionDate; } }

        public override int CreatorId { get { return _contentNode.CreatorId; } }
        public override string CreatorName { get { return GetProfileNameById(_contentNode.CreatorId); } }
        public override int WriterId { get { return _contentData.WriterId; } }
        public override string WriterName { get { return GetProfileNameById(_contentData.WriterId); } }

        public override bool IsDraft { get { return _contentData.Published == false; } }

        // beware what you use that one for - you don't want to cache its result
        private ICacheProvider GetAppropriateFacadeCache()
        {
            var facade = Facade.Current;
            var cache = facade == null
                ? null
                : ((_isPreviewing == false || FacadeService.FullCacheWhenPreviewing) && (ItemType != PublishedItemType.Member)
                    ? facade.SnapshotCache
                    : facade.FacadeCache);
            return cache;
        }

        public override IPublishedContent Parent
        {
            get
            {
                // have to use the "current" cache because a PublishedContent can be shared
                // amongst many snapshots and other content depend on the snapshots
                switch (_contentNode.ContentType.ItemType)
                {
                    case PublishedItemType.Content:
                        return GetContentById(_isPreviewing, _contentNode.ParentContentId);
                    case PublishedItemType.Media:
                        return GetMediaById(_isPreviewing, _contentNode.ParentContentId);
                    default:
                        throw new Exception("oops");
                }
            }
        }

        private string _childrenCacheKey;

        private string ChildrenCacheKey
        {
            get { return _childrenCacheKey ?? (_childrenCacheKey = CacheKeys.PublishedContentChildren(Key, _isPreviewing)); }
        }

        public override IEnumerable<IPublishedContent> Children
        {
            get
            {
                var cache = GetAppropriateFacadeCache();
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
                    c = GetContentByIds(_isPreviewing, _contentNode.ChildContentIds);
                    break;
                case PublishedItemType.Media:
                    c = GetMediaByIds(_isPreviewing, _contentNode.ChildContentIds);
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

        public override ICollection<IPublishedProperty> Properties { get { return _properties; } }

        public override IPublishedProperty GetProperty(string alias)
        {
            var index = _contentNode.ContentType.GetPropertyIndex(alias);
            var property = index < 0 ? null : _properties[index];
            return property;
        }

        public override IPublishedProperty GetProperty(string alias, bool recurse)
        {
            var property = GetProperty(alias);
            if (recurse == false) return property;

            var cache = GetAppropriateFacadeCache();
            if (cache == null)
                return base.GetProperty(alias, true);

            var key = ((Property) property).RecurseCacheKey;
            return (Property) cache.GetCacheItem(key, () => base.GetProperty(alias, true));
        }

        public override PublishedContentType ContentType
        {
            get { return _contentNode.ContentType; }
        }

        #endregion

        #region Internal

        // used by navigable content
        internal IPublishedProperty[] PropertiesArray { get { return _properties; } }

        // used by navigable content
        internal int ParentId { get { return _contentNode.ParentContentId; } }

        // used by navigable content
        // includes all children, published or unpublished
        // NavigableNavigator takes care of selecting those it wants
        internal IList<int> ChildIds { get { return _contentNode.ChildContentIds; } }

        // used by Property
        // gets a value indicating whether the content or media exists in
        // a previewing context or not, ie whether its Parent, Children, and
        // properties should refer to published, or draft content
        internal bool IsPreviewing { get { return _isPreviewing; } }

        private string _asPreviewingCacheKey;

        private string AsPreviewingCacheKey
        {
            get { return _asPreviewingCacheKey ?? (_asPreviewingCacheKey = CacheKeys.PublishedContentAsPreviewing(Key)); }
        }

        // used by ContentCache
        internal IPublishedContent AsPreviewingModel()
        {
            if (_isPreviewing)
                return this;

            var cache = GetAppropriateFacadeCache();
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
