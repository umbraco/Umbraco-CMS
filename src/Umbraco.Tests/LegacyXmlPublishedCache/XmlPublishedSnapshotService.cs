using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    /// <summary>
    /// Implements a published snapshot service.
    /// </summary>
    internal class XmlPublishedSnapshotService : IPublishedSnapshotService
    {
        private readonly XmlStore _xmlStore;
        private readonly RoutesCache _routesCache;
        private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
        private readonly PublishedContentTypeCache _contentTypeCache;
        private readonly IDomainService _domainService;
        private readonly IMediaService _mediaService;
        private readonly IUserService _userService;
        private readonly IAppCache _requestCache;
        private readonly GlobalSettings _globalSettings;
        private readonly IDefaultCultureAccessor _defaultCultureAccessor;
        private readonly IEntityXmlSerializer _entitySerializer;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        #region Constructors

        // used in WebBootManager + tests
        public XmlPublishedSnapshotService(
            ServiceContext serviceContext,
            IPublishedContentTypeFactory publishedContentTypeFactory,
            IScopeProvider scopeProvider,
            IAppCache requestCache,
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IVariationContextAccessor variationContextAccessor,
            IUmbracoContextAccessor umbracoContextAccessor,
            IDocumentRepository documentRepository,
            IMediaRepository mediaRepository,
            IMemberRepository memberRepository,
            IDefaultCultureAccessor defaultCultureAccessor,
            ILoggerFactory loggerFactory,
            GlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,
            IApplicationShutdownRegistry hostingLifetime,
            IShortStringHelper shortStringHelper,
            IEntityXmlSerializer entitySerializer,
            MainDom mainDom,
            bool testing = false,
            bool enableRepositoryEvents = true)
            : this(serviceContext, publishedContentTypeFactory, scopeProvider, requestCache,
                publishedSnapshotAccessor, variationContextAccessor, umbracoContextAccessor,
                documentRepository, mediaRepository, memberRepository,
                defaultCultureAccessor,
                loggerFactory, globalSettings, hostingEnvironment, hostingLifetime, shortStringHelper, entitySerializer, null, mainDom, testing, enableRepositoryEvents)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        // used in some tests
        internal XmlPublishedSnapshotService(
            ServiceContext serviceContext,
            IPublishedContentTypeFactory publishedContentTypeFactory,
            IScopeProvider scopeProvider,
            IAppCache requestCache,
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IVariationContextAccessor variationContextAccessor,
            IUmbracoContextAccessor umbracoContextAccessor,
            IDocumentRepository documentRepository,
            IMediaRepository mediaRepository,
            IMemberRepository memberRepository,
            IDefaultCultureAccessor defaultCultureAccessor,
            ILoggerFactory loggerFactory,
            GlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,
            IApplicationShutdownRegistry hostingLifetime,
            IShortStringHelper shortStringHelper,
            IEntityXmlSerializer entitySerializer,
            PublishedContentTypeCache contentTypeCache,
            MainDom mainDom,
            bool testing,
            bool enableRepositoryEvents)
        {
            _routesCache = new RoutesCache();
            _publishedContentTypeFactory = publishedContentTypeFactory;
            _contentTypeCache = contentTypeCache
                ?? new PublishedContentTypeCache(serviceContext.ContentTypeService, serviceContext.MediaTypeService, serviceContext.MemberTypeService, publishedContentTypeFactory, loggerFactory.CreateLogger<PublishedContentTypeCache>());

            _xmlStore = new XmlStore(serviceContext.ContentTypeService, serviceContext.ContentService, scopeProvider, _routesCache,
                _contentTypeCache, publishedSnapshotAccessor, mainDom, testing, enableRepositoryEvents,
                documentRepository, mediaRepository, memberRepository, entitySerializer, hostingEnvironment, hostingLifetime, shortStringHelper);

            _domainService = serviceContext.DomainService;
            _mediaService = serviceContext.MediaService;
            _userService = serviceContext.UserService;
            _defaultCultureAccessor = defaultCultureAccessor;
            _variationContextAccessor = variationContextAccessor;
            _requestCache = requestCache;
            _umbracoContextAccessor = umbracoContextAccessor;
            _globalSettings = globalSettings;
            _entitySerializer = entitySerializer;
        }

        public void Dispose()
        {
            _xmlStore.Dispose();
        }

        #endregion

        public IPublishedSnapshot CreatePublishedSnapshot(string previewToken)
        {
            // use _requestCache to store recursive properties lookup, etc. both in content
            // and media cache. Life span should be the current request. Or, ideally
            // the current caches, but that would mean creating an extra cache (StaticCache
            // probably) so better use RequestCache.

            var domainCache = new DomainCache(_domainService, _defaultCultureAccessor);

            return new PublishedSnapshot(
                new PublishedContentCache(_xmlStore, domainCache, _requestCache, _globalSettings, _contentTypeCache, _routesCache, _variationContextAccessor, previewToken),
                new PublishedMediaCache(_xmlStore, _mediaService, _userService, _requestCache, _contentTypeCache, _entitySerializer, _umbracoContextAccessor, _variationContextAccessor),
                new PublishedMemberCache(_contentTypeCache, _variationContextAccessor),
                domainCache);
        }

        #region Xml specific

        /// <summary>
        /// Gets the underlying XML store.
        /// </summary>
        public XmlStore XmlStore => _xmlStore;

        /// <summary>
        /// Gets the underlying RoutesCache.
        /// </summary>
        public RoutesCache RoutesCache => _routesCache;

        public bool VerifyContentAndPreviewXml()
        {
            return XmlStore.VerifyContentAndPreviewXml();
        }

        public void RebuildContentAndPreviewXml()
        {
            XmlStore.RebuildContentAndPreviewXml();
        }

        public bool VerifyMediaXml()
        {
            return XmlStore.VerifyMediaXml();
        }

        public void RebuildMediaXml()
        {
            XmlStore.RebuildMediaXml();
        }

        public bool VerifyMemberXml()
        {
            return XmlStore.VerifyMemberXml();
        }

        public void RebuildMemberXml()
        {
            XmlStore.RebuildMemberXml();
        }

        #endregion

        #region Change management

        public void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged)
        {
            _xmlStore.Notify(payloads, out draftChanged, out publishedChanged);
        }

        public void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged)
        {
            foreach (var payload in payloads)
                PublishedMediaCache.ClearCache(payload.Id);

            anythingChanged = true;
        }

        public void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads)
        {
            _xmlStore.Notify(payloads);
            if (payloads.Any(x => x.ItemType == typeof(IContentType).Name))
                _routesCache.Clear();
        }

        public void Notify(DataTypeCacheRefresher.JsonPayload[] payloads)
        {
            _publishedContentTypeFactory.NotifyDataTypeChanges(payloads.Select(x => x.Id).ToArray());
            _xmlStore.Notify(payloads);
        }

        public void Notify(DomainCacheRefresher.JsonPayload[] payloads)
        {
            _routesCache.Clear();
        }

        #endregion

        public void Rebuild(int groupSize = 5000, IReadOnlyCollection<int> contentTypeIds = null, IReadOnlyCollection<int> mediaTypeIds = null, IReadOnlyCollection<int> memberTypeIds = null) { }

        public Task CollectAsync() => Task.CompletedTask;
    }
}
