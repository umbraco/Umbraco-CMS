using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Cache;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    /// <summary>
    /// Implements a published snapshot service.
    /// </summary>
    internal class PublishedSnapshotService : PublishedSnapshotServiceBase
    {
        private readonly XmlStore _xmlStore;
        private readonly RoutesCache _routesCache;
        private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
        private readonly PublishedContentTypeCache _contentTypeCache;
        private readonly IDomainService _domainService;
        private readonly IMemberService _memberService;
        private readonly IMediaService _mediaService;
        private readonly IUserService _userService;
        private readonly ICacheProvider _requestCache;
        private readonly IGlobalSettings _globalSettings;
        private readonly IDefaultCultureAccessor _defaultCultureAccessor;
        private readonly ISiteDomainHelper _siteDomainHelper;
        private readonly IEntityXmlSerializer _entitySerializer;

        #region Constructors

        // used in WebBootManager + tests
        public PublishedSnapshotService(ServiceContext serviceContext,
            IPublishedContentTypeFactory publishedContentTypeFactory,
            IScopeProvider scopeProvider,
            ICacheProvider requestCache,
            IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor,
            IDocumentRepository documentRepository, IMediaRepository mediaRepository, IMemberRepository memberRepository,
            IDefaultCultureAccessor defaultCultureAccessor,
            ILogger logger,
            IGlobalSettings globalSettings,
            ISiteDomainHelper siteDomainHelper,
            IEntityXmlSerializer entitySerializer,
            MainDom mainDom,
            bool testing = false, bool enableRepositoryEvents = true)
            : this(serviceContext, publishedContentTypeFactory, scopeProvider, requestCache, 
                publishedSnapshotAccessor, variationContextAccessor,
                documentRepository, mediaRepository, memberRepository,
                defaultCultureAccessor,
                logger, globalSettings, siteDomainHelper, entitySerializer, null, mainDom, testing, enableRepositoryEvents)
        { }

        // used in some tests
        internal PublishedSnapshotService(ServiceContext serviceContext,
            IPublishedContentTypeFactory publishedContentTypeFactory,
            IScopeProvider scopeProvider,
            ICacheProvider requestCache,
            IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor,
            IDocumentRepository documentRepository, IMediaRepository mediaRepository, IMemberRepository memberRepository,
            IDefaultCultureAccessor defaultCultureAccessor,
            ILogger logger,
            IGlobalSettings globalSettings,
            ISiteDomainHelper siteDomainHelper,
            IEntityXmlSerializer entitySerializer,
            PublishedContentTypeCache contentTypeCache,
            MainDom mainDom,
            bool testing, bool enableRepositoryEvents)
            : base(publishedSnapshotAccessor, variationContextAccessor)
        {
            _routesCache = new RoutesCache();
            _publishedContentTypeFactory = publishedContentTypeFactory;
            _contentTypeCache = contentTypeCache
                ?? new PublishedContentTypeCache(serviceContext.ContentTypeService, serviceContext.MediaTypeService, serviceContext.MemberTypeService, publishedContentTypeFactory, logger);

            _xmlStore = new XmlStore(serviceContext.ContentTypeService, serviceContext.ContentService, scopeProvider, _routesCache,
                _contentTypeCache, publishedSnapshotAccessor, mainDom, testing, enableRepositoryEvents,
                documentRepository, mediaRepository, memberRepository, globalSettings, entitySerializer);

            _domainService = serviceContext.DomainService;
            _memberService = serviceContext.MemberService;
            _mediaService = serviceContext.MediaService;
            _userService = serviceContext.UserService;
            _defaultCultureAccessor = defaultCultureAccessor;

            _requestCache = requestCache;
            _globalSettings = globalSettings;
            _siteDomainHelper = siteDomainHelper;
            _entitySerializer = entitySerializer;
        }

        public override void Dispose()
        {
            _xmlStore.Dispose();
        }

        #endregion

        #region Environment

        public override bool EnsureEnvironment(out IEnumerable<string> errors)
        {
            // Test creating/saving/deleting a file in the same location as the content xml file
            // NOTE: We cannot modify the xml file directly because a background thread is responsible for
            // that and we might get lock issues.
            try
            {
                XmlStore.EnsureFilePermission();
                errors = Enumerable.Empty<string>();
                return true;
            }
            catch
            {
                errors = new[] { SystemFiles.GetContentCacheXml(_globalSettings) };
                return false;
            }
        }

        #endregion

        #region Caches

        public override IPublishedSnapshot CreatePublishedSnapshot(string previewToken)
        {
            // use _requestCache to store recursive properties lookup, etc. both in content
            // and media cache. Life span should be the current request. Or, ideally
            // the current caches, but that would mean creating an extra cache (StaticCache
            // probably) so better use RequestCache.

            var domainCache = new DomainCache(_domainService, _defaultCultureAccessor);

            return new PublishedSnapshot(
                new PublishedContentCache(_xmlStore, domainCache, _requestCache, _globalSettings, _siteDomainHelper, _contentTypeCache, _routesCache, previewToken),
                new PublishedMediaCache(_xmlStore, _mediaService, _userService, _requestCache, _contentTypeCache, _entitySerializer),
                new PublishedMemberCache(_xmlStore, _requestCache, _memberService, _contentTypeCache),
                domainCache);
        }

        #endregion

        #region Preview

        public override string EnterPreview(IUser user, int contentId)
        {
            var previewContent = new PreviewContent(_xmlStore, user.Id);
            previewContent.CreatePreviewSet(contentId, true); // preview branch below that content
            return previewContent.Token;
            //previewContent.ActivatePreviewCookie();
        }

        public override void RefreshPreview(string previewToken, int contentId)
        {
            if (previewToken.IsNullOrWhiteSpace()) return;
            var previewContent = new PreviewContent(_xmlStore, previewToken);
            previewContent.CreatePreviewSet(contentId, true); // preview branch below that content
        }

        public override void ExitPreview(string previewToken)
        {
            if (previewToken.IsNullOrWhiteSpace()) return;
            var previewContent = new PreviewContent(_xmlStore, previewToken);
            previewContent.ClearPreviewSet();
        }

        #endregion

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

        public override void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged)
        {
            _xmlStore.Notify(payloads, out draftChanged, out publishedChanged);
        }

        public override void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged)
        {
            foreach (var payload in payloads)
                PublishedMediaCache.ClearCache(payload.Id);

            anythingChanged = true;
        }

        public override void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads)
        {
            _xmlStore.Notify(payloads);
            if (payloads.Any(x => x.ItemType == typeof(IContentType).Name))
                _routesCache.Clear();
        }

        public override void Notify(DataTypeCacheRefresher.JsonPayload[] payloads)
        {
            _publishedContentTypeFactory.NotifyDataTypeChanges(payloads.Select(x => x.Id).ToArray());
            _xmlStore.Notify(payloads);
        }

        public override void Notify(DomainCacheRefresher.JsonPayload[] payloads)
        {
            _routesCache.Clear();
        }

        #endregion
    }
}
