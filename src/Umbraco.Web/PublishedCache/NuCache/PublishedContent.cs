using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
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
        private readonly string _urlSegment;

        #region Constructors

        public PublishedContent(
            ContentNode contentNode,
            ContentData contentData,
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IVariationContextAccessor variationContextAccessor,
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(umbracoContextAccessor)
        {
            _contentNode = contentNode ?? throw new ArgumentNullException(nameof(contentNode));
            ContentData = contentData ?? throw new ArgumentNullException(nameof(contentData));
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            VariationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));

            _urlSegment = ContentData.UrlSegment;
            IsPreviewing = ContentData.Published == false;

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
                : (string)cache.Get(CacheKeys.ProfileName(id), () => GetProfileNameByIdNoCache(id));
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

        // used when cloning in ContentNode
        public PublishedContent(ContentNode contentNode, PublishedContent origin)
            : base(origin.UmbracoContextAccessor)
        {
            _contentNode = contentNode;
            _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
            VariationContextAccessor = origin.VariationContextAccessor;
            ContentData = origin.ContentData;

            _urlSegment = origin._urlSegment;
            IsPreviewing = origin.IsPreviewing;

            // here is the main benefit: we do not re-create properties so if anything
            // is cached locally, we share the cache - which is fine - if anything depends
            // on the tree structure, it should not be cached locally to begin with
            PropertiesArray = origin.PropertiesArray;
        }

        // clone for previewing as draft a published content that is published and has no draft
        private PublishedContent(
            PublishedContent origin,
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(umbracoContextAccessor)
        {
            _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
            VariationContextAccessor = origin.VariationContextAccessor;
            _contentNode = origin._contentNode;
            ContentData = origin.ContentData;

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

        private Func<IPublishedSnapshot, bool, int, IPublishedContent> GetGetterById(PublishedItemType itemType)
        {
            switch (ContentType.ItemType)
            {
                case PublishedItemType.Content:
                    return GetContentByIdFunc;
                case PublishedItemType.Media:
                    return GetMediaByIdFunc;
                default:
                    throw new Exception("panic: invalid item type");
            }

        }

        #endregion

        #region Content Type

        /// <inheritdoc />
        public override IPublishedContentType ContentType => _contentNode.ContentType;

        #endregion

        #region PublishedElement

        /// <inheritdoc />
        public override Guid Key => _contentNode.Uid;

        #endregion

        #region PublishedContent

        internal ContentData ContentData { get; }

        /// <inheritdoc />
        public override int Id => _contentNode.Id;

        /// <inheritdoc />
        public override string Name(string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!ContentType.VariesByCulture())
                return ContentData.Name;

            // handle context culture for variant
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            // get
            return culture != "" && ContentData.CultureInfos.TryGetValue(culture, out var infos) ? infos.Name : null;
        }

        /// <inheritdoc />
        public override string UrlSegment(string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!ContentType.VariesByCulture())
                return _urlSegment;

            // handle context culture for variant
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            // get
            return ContentData.CultureInfos.TryGetValue(culture, out var infos) ? infos.UrlSegment : null;
        }

        /// <inheritdoc />
        public override int SortOrder => _contentNode.SortOrder;

        /// <inheritdoc />
        public override int Level => _contentNode.Level;

        /// <inheritdoc />
        public override string Path => _contentNode.Path;

        /// <inheritdoc />
        public override int? TemplateId => ContentData.TemplateId;

        /// <inheritdoc />
        public override int CreatorId => _contentNode.CreatorId;

        /// <inheritdoc />
        public override string CreatorName => GetProfileNameById(_contentNode.CreatorId);

        /// <inheritdoc />
        public override DateTime CreateDate => _contentNode.CreateDate;

        /// <inheritdoc />
        public override int WriterId => ContentData.WriterId;

        /// <inheritdoc />
        public override string WriterName => GetProfileNameById(ContentData.WriterId);

        /// <inheritdoc />
        public override DateTime UpdateDate => ContentData.VersionDate;

        /// <inheritdoc />
        public override DateTime CultureDate(string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!ContentType.VariesByCulture())
                return UpdateDate;

            // handle context culture for variant
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            // get
            return culture != "" && ContentData.CultureInfos.TryGetValue(culture, out var infos) ? infos.Date : DateTime.MinValue;
        }

        // ReSharper disable once CollectionNeverUpdated.Local
        private static readonly List<string> EmptyListOfString = new List<string>();
        private IReadOnlyCollection<string> _cultures;

        /// <inheritdoc />
        public override IReadOnlyCollection<string> Cultures
        {
            get
            {
                if (!ContentType.VariesByCulture())
                    return EmptyListOfString;

                return _cultures ?? (_cultures = new HashSet<string>(ContentData.CultureInfos.Keys, StringComparer.OrdinalIgnoreCase));
            }
        }

        /// <inheritdoc />
        public override bool IsDraft(string culture = null)
        {
            // if this is the 'published' published content, nothing can be draft
            if (ContentData.Published)
                return false;

            // not the 'published' published content, and does not vary = must be draft
            if (!ContentType.VariesByCulture())
                return true;

            // handle context culture
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            // not the 'published' published content, and varies
            // = depends on the culture
            return ContentData.CultureInfos.TryGetValue(culture, out var cvar) && cvar.IsDraft;
        }

        /// <inheritdoc />
        public override bool IsPublished(string culture = null)
        {
            // whether we are the 'draft' or 'published' content, need to determine whether
            // there is a 'published' version for the specified culture (or at all, for
            // invariant content items)

            // if there is no 'published' published content, no culture can be published
            var hasPublished = _contentNode.PublishedContent != null;
            if (!hasPublished)
                return false;

            // if there is a 'published' published content, and does not vary = published
            if (!ContentType.VariesByCulture())
                return true;

            // handle context culture
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            // there is a 'published' published content, and varies
            // = depends on the culture
            return _contentNode.PublishedContent.ContentData.CultureInfos.ContainsKey(culture);
        }

        #endregion

        #region Tree

        /// <inheritdoc />
        public override IPublishedContent Parent()
        {
            var getById = GetGetterById(ContentType.ItemType);
            var publishedSnapshot = _publishedSnapshotAccessor.PublishedSnapshot;
            return getById(publishedSnapshot, IsPreviewing, ParentId);
        }

        /// <inheritdoc />
        public override IEnumerable<IPublishedContent> Children(string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!ContentType.VariesByCulture() && culture != "*")
                culture = "";

            // handle context culture for variant
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            var getById = GetGetterById(ContentType.ItemType);
            var publishedSnapshot = _publishedSnapshotAccessor.PublishedSnapshot;
            var id = _contentNode.FirstChildContentId;

            while (id > 0)
            {
                var content = getById(publishedSnapshot, IsPreviewing, id);
                if (content == null)
                    throw new Exception("panic: failed to get content");

                if (culture == "*" || content.IsInvariantOrHasCulture(culture))
                    yield return content;

                id = UnwrapIPublishedContent(content)._contentNode.NextSiblingContentId;
            }
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
        private IAppCache GetAppropriateCache()
        {
            var publishedSnapshot = (PublishedSnapshot)_publishedSnapshotAccessor.PublishedSnapshot;
            var cache = publishedSnapshot == null
                ? null
                : ((IsPreviewing == false || PublishedSnapshotService.FullCacheWhenPreviewing) && (ContentType.ItemType != PublishedItemType.Member)
                    ? publishedSnapshot.ElementsCache
                    : publishedSnapshot.SnapshotCache);
            return cache;
        }

        private IAppCache GetCurrentSnapshotCache()
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
        // note: this is not efficient - we do not try to be (would require a double-linked list)
        internal IList<int> ChildIds => Children().Select(x => x.Id).ToList();

        // used by Property
        // gets a value indicating whether the content or media exists in
        // a previewing context or not, ie whether its Parent, Children, and
        // properties should refer to published, or draft content
        internal bool IsPreviewing { get; }

        private string _asPreviewingCacheKey;

        private string AsPreviewingCacheKey => _asPreviewingCacheKey ?? (_asPreviewingCacheKey = CacheKeys.PublishedContentAsPreviewing(Key));

        // used by ContentCache
        internal IPublishedContent AsDraft()
        {
            if (IsPreviewing)
                return this;

            var cache = GetAppropriateCache();
            if (cache == null) return new PublishedContent(this, UmbracoContextAccessor).CreateModel();
            return (IPublishedContent)cache.Get(AsPreviewingCacheKey, () => new PublishedContent(this, UmbracoContextAccessor).CreateModel());
        }

        // used by Navigable.Source,...
        internal static PublishedContent UnwrapIPublishedContent(IPublishedContent content)
        {
            while (content is PublishedContentWrapped wrapped)
                content = wrapped.Unwrap();
            if (!(content is PublishedContent inner))
                throw new InvalidOperationException("Innermost content is not PublishedContent.");
            return inner;
        }

        #endregion
    }
}
