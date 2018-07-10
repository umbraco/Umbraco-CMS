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

        private readonly string _urlSegment;

        #region Constructors

        public PublishedContent(ContentNode contentNode, ContentData contentData, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor)
        {
            _contentNode = contentNode;
            _contentData = contentData;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            VariationContextAccessor = variationContextAccessor;

            _urlSegment = _contentData.Name.ToUrlSegment();
            IsPreviewing = _contentData.Published == false;

            var properties = new List<IPublishedProperty>();
            foreach (var propertyType in _contentNode.ContentType.PropertyTypes)
            {
                // add one property per property type - this is required, for the indexing to work
                // if contentData supplies pdatas, use them, else use null
                contentData.Properties.TryGetValue(propertyType.Alias, out var pdatas); // else will be null
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
            VariationContextAccessor = origin.VariationContextAccessor;
            _contentData = origin._contentData;

            _urlSegment = origin._urlSegment;
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
            VariationContextAccessor = origin.VariationContextAccessor;
            _contentNode = origin._contentNode;
            _contentData = origin._contentData;

            _urlSegment = origin._urlSegment;
            IsPreviewing = true;

            // clone properties so _isPreviewing is true
            PropertiesArray = origin.PropertiesArray.Select(x => (IPublishedProperty)new Property((Property)x, this)).ToArray();
        }

        #endregion

        #region Get Content/Media for Parent/Children

        // this is for tests purposes
        // args are: current published snapshot (may be null), previewing, content id - returns: content

        internal static Func<IPublishedSnapshot, bool, int, IPublishedContent> GetContentByIdFunc { get; set; }
            = (publishedShapshot, previewing, id) => publishedShapshot.Content.GetById(previewing, id);

        internal static Func<IPublishedSnapshot, bool, int, IPublishedContent> GetMediaByIdFunc { get; set; }
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

        #region Content Type

        /// <inheritdoc />
        public override PublishedContentType ContentType => _contentNode.ContentType;

        #endregion

        #region PublishedElement

        /// <inheritdoc />
        public override Guid Key => _contentNode.Uid;

        #endregion

        #region PublishedContent

        /// <inheritdoc />
        public override int Id => _contentNode.Id;

        /// <inheritdoc />
        public override string Name
        {
            get
            {
                if (!ContentType.VariesByCulture())
                    return _contentData.Name;

                var culture = VariationContextAccessor?.VariationContext?.Culture ?? "";
                if (culture == "")
                    return _contentData.Name;

                return Cultures.TryGetValue(culture, out var cultureInfos) ? cultureInfos.Name : null;
            }
        }

        /// <inheritdoc />
        public override string UrlSegment
        {
            get
            {
                if (!ContentType.VariesByCulture())
                    return _urlSegment;

                var culture = VariationContextAccessor?.VariationContext?.Culture ?? "";
                if (culture == "")
                    return _urlSegment;

                return Cultures.TryGetValue(culture, out var cultureInfos) ? cultureInfos.UrlSegment : null;
            }
        }

        /// <inheritdoc />
        public override int SortOrder => _contentNode.SortOrder;

        /// <inheritdoc />
        public override int Level => _contentNode.Level;

        /// <inheritdoc />
        public override string Path => _contentNode.Path;

        /// <inheritdoc />
        public override int TemplateId => _contentData.TemplateId;

        /// <inheritdoc />
        public override int CreatorId => _contentNode.CreatorId;

        /// <inheritdoc />
        public override string CreatorName => GetProfileNameById(_contentNode.CreatorId);

        /// <inheritdoc />
        public override DateTime CreateDate => _contentNode.CreateDate;

        /// <inheritdoc />
        public override int WriterId => _contentData.WriterId;

        /// <inheritdoc />
        public override string WriterName => GetProfileNameById(_contentData.WriterId);

        /// <inheritdoc />
        public override DateTime UpdateDate => _contentData.VersionDate;

        private IReadOnlyDictionary<string, PublishedCultureInfo> _cultureInfos;

        private static readonly IReadOnlyDictionary<string, PublishedCultureInfo> NoCultureInfos = new Dictionary<string, PublishedCultureInfo>();

        /// <inheritdoc />
        public override PublishedCultureInfo GetCulture(string culture = null)
        {
            // handle context culture
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            // no invariant culture infos
            if (culture == "") return null;

            // get
            return Cultures.TryGetValue(culture, out var cultureInfos) ? cultureInfos : null;
        }

        /// <inheritdoc />
        public override IReadOnlyDictionary<string, PublishedCultureInfo> Cultures
        {
            get
            {
                if (!ContentType.VariesByCulture())
                    return NoCultureInfos;

                if (_cultureInfos != null) return _cultureInfos;

                if (_contentData.CultureInfos == null)
                    throw new Exception("oops: _contentDate.CultureInfos is null.");
                return _cultureInfos = _contentData.CultureInfos
                    .ToDictionary(x => x.Key, x => new PublishedCultureInfo(x.Key, x.Value.Name, x.Value.Date), StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <inheritdoc />
        public override PublishedItemType ItemType => _contentNode.ContentType.ItemType;

        /// <inheritdoc />
        public override bool IsDraft => _contentData.Published == false;

        #endregion

        #region Tree

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        private string _childrenCacheKey;

        private string ChildrenCacheKey => _childrenCacheKey ?? (_childrenCacheKey = CacheKeys.PublishedContentChildren(Key, IsPreviewing));

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

        #endregion

        #region Properties


        /// <inheritdoc cref="IPublishedElement.Properties"/>
        public override IEnumerable<IPublishedProperty> Properties => PropertiesArray;

        /// <inheritdoc cref="IPublishedElement.GetProperty(string)"/>
        public override IPublishedProperty GetProperty(string alias)
        {
            var index = _contentNode.ContentType.GetPropertyIndex(alias);
            if (index < 0) return null; // happens when 'alias' does not match a content type property alias
            if (index >= PropertiesArray.Length) // should never happen - properties array must be in sync with property type
                throw new IndexOutOfRangeException("Index points outside the properties array, which means the properties array is corrupt.");
            var property = PropertiesArray[index];
            return property;
        }

        #endregion

        #region Caching

        // beware what you use that one for - you don't want to cache its result
        private ICacheProvider GetAppropriateCache()
        {
            var publishedSnapshot = (PublishedSnapshot)_publishedSnapshotAccessor.PublishedSnapshot;
            var cache = publishedSnapshot == null
                ? null
                : ((IsPreviewing == false || PublishedSnapshotService.FullCacheWhenPreviewing) && (ItemType != PublishedItemType.Member)
                    ? publishedSnapshot.ElementsCache
                    : publishedSnapshot.SnapshotCache);
            return cache;
        }

        private ICacheProvider GetCurrentSnapshotCache()
        {
            var publishedSnapshot = (PublishedSnapshot)_publishedSnapshotAccessor.PublishedSnapshot;
            return publishedSnapshot?.SnapshotCache;
        }

        #endregion

        #region Internal

        // used by property
        internal IVariationContextAccessor VariationContextAccessor { get; }

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
