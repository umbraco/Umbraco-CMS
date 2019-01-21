using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CSharpTest.Net.Collections;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Cache;
using Umbraco.Web.Install;
using Umbraco.Web.PublishedCache.NuCache.DataSource;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PublishedCache.NuCache
{
    class PublishedSnapshotService : PublishedSnapshotServiceBase
    {
        private readonly ServiceContext _serviceContext;
        private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
        private readonly IScopeProvider _scopeProvider;
        private readonly IDataSource _dataSource;
        private readonly ILogger _logger;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IGlobalSettings _globalSettings;
        private readonly ISiteDomainHelper _siteDomainHelper;
        private readonly IEntityXmlSerializer _entitySerializer;
        private readonly IDefaultCultureAccessor _defaultCultureAccessor;

        // volatile because we read it with no lock
        private volatile bool _isReady;

        private readonly ContentStore _contentStore;
        private readonly ContentStore _mediaStore;
        private readonly SnapDictionary<int, Domain> _domainStore;
        private readonly object _storesLock = new object();

        private BPlusTree<int, ContentNodeKit> _localContentDb;
        private BPlusTree<int, ContentNodeKit> _localMediaDb;
        private readonly bool _localDbExists;

        // define constant - determines whether to use cache when previewing
        // to store eg routes, property converted values, anything - caching
        // means faster execution, but uses memory - not sure if we want it
        // so making it configureable.
        public static readonly bool FullCacheWhenPreviewing = true;

        // define constant - determines whether to cache the published content
        // objects (in the elements cache, or snapshot cache, depending on preview)
        // or to refetch them all the time. caching is faster but uses more
        // memory. not sure what we want.
        public static readonly bool CachePublishedContentChildren = true;

        // define constant - determines whether to cache the content cache root
        // objects (in the elements cache, or snapshot cache, depending on preview)
        // or to refecth them all the time. caching is faster but uses more
        // memory - not sure what we want.
        public static readonly bool CacheContentCacheRoots = true;

        #region Constructors

        //private static int _singletonCheck;

        public PublishedSnapshotService(Options options, IMainDom mainDom, IRuntimeState runtime,
            ServiceContext serviceContext, IPublishedContentTypeFactory publishedContentTypeFactory, IdkMap idkMap,
            IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor,
            ILogger logger, IScopeProvider scopeProvider,
            IDocumentRepository documentRepository, IMediaRepository mediaRepository, IMemberRepository memberRepository,
            IDefaultCultureAccessor defaultCultureAccessor,
            IDataSource dataSource, IGlobalSettings globalSettings, ISiteDomainHelper siteDomainHelper,
            IEntityXmlSerializer entitySerializer)
            : base(publishedSnapshotAccessor, variationContextAccessor)
        {
            //if (Interlocked.Increment(ref _singletonCheck) > 1)
            //    throw new Exception("Singleton must be instancianted only once!");

            _serviceContext = serviceContext;
            _publishedContentTypeFactory = publishedContentTypeFactory;
            _dataSource = dataSource;
            _logger = logger;
            _scopeProvider = scopeProvider;
            _documentRepository = documentRepository;
            _mediaRepository = mediaRepository;
            _memberRepository = memberRepository;
            _defaultCultureAccessor = defaultCultureAccessor;
            _globalSettings = globalSettings;
            _siteDomainHelper = siteDomainHelper;

            // we need an Xml serializer here so that the member cache can support XPath,
            // for members this is done by navigating the serialized-to-xml member
            _entitySerializer = entitySerializer;

            // we always want to handle repository events, configured or not
            // assuming no repository event will trigger before the whole db is ready
            // (ideally we'd have Upgrading.App vs Upgrading.Data application states...)
            InitializeRepositoryEvents();

            // however, the cache is NOT available until we are configured, because loading
            // content (and content types) from database cannot be consistent (see notes in "Handle
            // Notifications" region), so
            // - notifications will be ignored
            // - trying to obtain a published snapshot from the service will throw
            if (runtime.Level != RuntimeLevel.Run)
                return;

            if (options.IgnoreLocalDb == false)
            {
                var registered = mainDom.Register(
                    null,
                    () =>
                    {
                        lock (_storesLock)
                        {
                            _contentStore.ReleaseLocalDb();
                            _localContentDb = null;
                            _mediaStore.ReleaseLocalDb();
                            _localMediaDb = null;
                        }
                    });

                if (registered)
                {
                    var localContentDbPath = IOHelper.MapPath("~/App_Data/NuCache.Content.db");
                    var localMediaDbPath = IOHelper.MapPath("~/App_Data/NuCache.Media.db");
                    _localDbExists = System.IO.File.Exists(localContentDbPath) && System.IO.File.Exists(localMediaDbPath);

                    // if both local dbs exist then GetTree will open them, else new dbs will be created
                    _localContentDb = BTree.GetTree(localContentDbPath, _localDbExists);
                    _localMediaDb = BTree.GetTree(localMediaDbPath, _localDbExists);
                }

                // stores are created with a db so they can write to it, but they do not read from it,
                // stores need to be populated, happens in OnResolutionFrozen which uses _localDbExists to
                // figure out whether it can read the dbs or it should populate them from sql
                _contentStore = new ContentStore(publishedSnapshotAccessor, variationContextAccessor, logger, _localContentDb);
                _mediaStore = new ContentStore(publishedSnapshotAccessor, variationContextAccessor, logger, _localMediaDb);
            }
            else
            {
                _contentStore = new ContentStore(publishedSnapshotAccessor, variationContextAccessor, logger);
                _mediaStore = new ContentStore(publishedSnapshotAccessor, variationContextAccessor, logger);
            }

            _domainStore = new SnapDictionary<int, Domain>();

            LoadCaches();

            if (idkMap != null)
            {
                idkMap.SetMapper(UmbracoObjectTypes.Document, id => _contentStore.LiveSnapshot.Get(id).Uid, uid => _contentStore.LiveSnapshot.Get(uid).Id);
                idkMap.SetMapper(UmbracoObjectTypes.Media, id => _mediaStore.LiveSnapshot.Get(id).Uid, uid => _mediaStore.LiveSnapshot.Get(uid).Id);
            }
        }

        private void LoadCaches()
        {
            lock (_storesLock)
            {
                // populate the stores

                try
                {
                    if (_localDbExists)
                    {
                        LockAndLoadContent(LoadContentFromLocalDbLocked);
                        LockAndLoadMedia(LoadMediaFromLocalDbLocked);
                    }
                    else
                    {
                        LockAndLoadContent(LoadContentFromDatabaseLocked);
                        LockAndLoadMedia(LoadMediaFromDatabaseLocked);
                    }

                    LockAndLoadDomains();
                }
                catch (Exception ex)
                {
                    _logger.Fatal<PublishedSnapshotService>(ex, "Panic, exception while loading cache data.");
                }

                // finaly, cache is ready!
                _isReady = true;
            }
        }

        private void InitializeRepositoryEvents()
        {
            //fixme: The reason these events are in the repository is for legacy, the events should exist at the service
            // level now since we can fire these events within the transaction... so move the events to service level

            // plug repository event handlers
            // these trigger within the transaction to ensure consistency
            // and are used to maintain the central, database-level XML cache
            DocumentRepository.ScopeEntityRemove += OnContentRemovingEntity;
            //ContentRepository.RemovedVersion += OnContentRemovedVersion;
            DocumentRepository.ScopedEntityRefresh += OnContentRefreshedEntity;
            MediaRepository.ScopeEntityRemove += OnMediaRemovingEntity;
            //MediaRepository.RemovedVersion += OnMediaRemovedVersion;
            MediaRepository.ScopedEntityRefresh += OnMediaRefreshedEntity;
            MemberRepository.ScopeEntityRemove += OnMemberRemovingEntity;
            //MemberRepository.RemovedVersion += OnMemberRemovedVersion;
            MemberRepository.ScopedEntityRefresh += OnMemberRefreshedEntity;

            // plug
            ContentTypeService.ScopedRefreshedEntity += OnContentTypeRefreshedEntity;
            MediaTypeService.ScopedRefreshedEntity += OnMediaTypeRefreshedEntity;
            MemberTypeService.ScopedRefreshedEntity += OnMemberTypeRefreshedEntity;
        }

        private void TearDownRepositoryEvents()
        {
            DocumentRepository.ScopeEntityRemove -= OnContentRemovingEntity;
            //ContentRepository.RemovedVersion -= OnContentRemovedVersion;
            DocumentRepository.ScopedEntityRefresh -= OnContentRefreshedEntity;
            MediaRepository.ScopeEntityRemove -= OnMediaRemovingEntity;
            //MediaRepository.RemovedVersion -= OnMediaRemovedVersion;
            MediaRepository.ScopedEntityRefresh -= OnMediaRefreshedEntity;
            MemberRepository.ScopeEntityRemove -= OnMemberRemovingEntity;
            //MemberRepository.RemovedVersion -= OnMemberRemovedVersion;
            MemberRepository.ScopedEntityRefresh -= OnMemberRefreshedEntity;

            ContentTypeService.ScopedRefreshedEntity -= OnContentTypeRefreshedEntity;
            MediaTypeService.ScopedRefreshedEntity -= OnMediaTypeRefreshedEntity;
            MemberTypeService.ScopedRefreshedEntity -= OnMemberTypeRefreshedEntity;
        }

        public override void Dispose()
        {
            TearDownRepositoryEvents();
            base.Dispose();
        }

        public class Options
        {
            // disabled: prevents the published snapshot from updating and exposing changes
            //           or even creating a new published snapshot to see changes, uses old cache = bad
            //
            //// indicates that the snapshot cache should reuse the application request cache
            //// otherwise a new cache object would be created for the snapshot specifically,
            //// which is the default - web boot manager uses this to optimze facades
            //public bool PublishedSnapshotCacheIsApplicationRequestCache;

            public bool IgnoreLocalDb;
        }

        #endregion

        #region Environment

        public override bool EnsureEnvironment(out IEnumerable<string> errors)
        {
            // must have app_data and be able to write files into it
            var ok = FilePermissionHelper.TryCreateDirectory(SystemDirectories.Data);
            errors = ok ? Enumerable.Empty<string>() : new[] { "NuCache local DB files." };
            return ok;
        }

        #endregion

        #region Populate Stores

        // sudden panic... but in RepeatableRead can a content that I haven't already read, be removed
        // before I read it? NO! because the WHOLE content tree is read-locked using WithReadLocked.
        // don't panic.

        private void LockAndLoadContent(Action<IScope> action)
        {
            using (_contentStore.GetWriter(_scopeProvider))
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.ContentTree);
                    action(scope);
                    scope.Complete();
                }
            }
        }

        private void LoadContentFromDatabaseLocked(IScope scope)
        {
            // locks:
            // contentStore is wlocked (1 thread)
            // content (and types) are read-locked

            var contentTypes = _serviceContext.ContentTypeService.GetAll()
                .Select(x => _publishedContentTypeFactory.CreateContentType(x));
            _contentStore.UpdateContentTypes(null, contentTypes, null);

            _localContentDb?.Clear();

            _logger.Debug<PublishedSnapshotService>("Loading content from database...");
            var sw = Stopwatch.StartNew();
            var kits = _dataSource.GetAllContentSources(scope);
            _contentStore.SetAll(kits);
            sw.Stop();
            _logger.Debug<PublishedSnapshotService>("Loaded content from database ({Duration}ms)", sw.ElapsedMilliseconds);
        }

        private void LoadContentFromLocalDbLocked(IScope scope)
        {
            var contentTypes = _serviceContext.ContentTypeService.GetAll()
                .Select(x => _publishedContentTypeFactory.CreateContentType(x));
            _contentStore.UpdateContentTypes(null, contentTypes, null);

            _logger.Debug<PublishedSnapshotService>("Loading content from local db...");
            var sw = Stopwatch.StartNew();
            var kits = _localContentDb.Select(x => x.Value).OrderBy(x => x.Node.Level);
            _contentStore.SetAll(kits);
            sw.Stop();
            _logger.Debug<PublishedSnapshotService>("Loaded content from local db ({Duration}ms)", sw.ElapsedMilliseconds);
        }

        // keep these around - might be useful

        //private void LoadContentBranch(IContent content)
        //{
        //    LoadContent(content);

        //    foreach (var child in content.Children())
        //        LoadContentBranch(child);
        //}

        //private void LoadContent(IContent content)
        //{
        //    var contentService = _serviceContext.ContentService as ContentService;
        //    var newest = content;
        //    var published = newest.Published
        //        ? newest
        //        : (newest.HasPublishedVersion ? contentService.GetByVersion(newest.PublishedVersionGuid) : null);

        //    var contentNode = CreateContentNode(newest, published);
        //    _contentStore.Set(contentNode);
        //}

        private void LockAndLoadMedia(Action<IScope> action)
        {
            using (_mediaStore.GetWriter(_scopeProvider))
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.MediaTree);
                    action(scope);
                    scope.Complete();
                }
            }
        }

        private void LoadMediaFromDatabaseLocked(IScope scope)
        {
            // locks & notes: see content

            var mediaTypes = _serviceContext.MediaTypeService.GetAll()
                .Select(x => _publishedContentTypeFactory.CreateContentType(x));
            _mediaStore.UpdateContentTypes(null, mediaTypes, null);

            _localMediaDb?.Clear();

            _logger.Debug<PublishedSnapshotService>("Loading media from database...");
            var sw = Stopwatch.StartNew();
            var kits = _dataSource.GetAllMediaSources(scope);
            _mediaStore.SetAll(kits);
            sw.Stop();
            _logger.Debug<PublishedSnapshotService>("Loaded media from database ({Duration}ms)", sw.ElapsedMilliseconds);
        }

        private void LoadMediaFromLocalDbLocked(IScope scope)
        {
            var mediaTypes = _serviceContext.MediaTypeService.GetAll()
                .Select(x => _publishedContentTypeFactory.CreateContentType(x));
            _mediaStore.UpdateContentTypes(null, mediaTypes, null);

            _logger.Debug<PublishedSnapshotService>("Loading media from local db...");
            var sw = Stopwatch.StartNew();
            var kits = _localMediaDb.Select(x => x.Value);
            _mediaStore.SetAll(kits);
            sw.Stop();
            _logger.Debug<PublishedSnapshotService>("Loaded media from local db ({Duration}ms)", sw.ElapsedMilliseconds);
        }

        // keep these around - might be useful

        //private void LoadMediaBranch(IMedia media)
        //{
        //    LoadMedia(media);

        //    foreach (var child in media.Children())
        //        LoadMediaBranch(child);
        //}

        //private void LoadMedia(IMedia media)
        //{
        //    var mediaType = _contentTypeCache.Get(PublishedItemType.Media, media.ContentTypeId);

        //    var mediaData = new ContentData
        //    {
        //        Name = media.Name,
        //        Published = true,
        //        Version = media.Version,
        //        VersionDate = media.UpdateDate,
        //        WriterId = media.CreatorId, // what else?
        //        TemplateId = -1, // have none
        //        Properties = GetPropertyValues(media)
        //    };

        //    var mediaNode = new ContentNode(media.Id, mediaType,
        //        media.Level, media.Path, media.SortOrder,
        //        media.ParentId, media.CreateDate, media.CreatorId,
        //        null, mediaData);

        //    _mediaStore.Set(mediaNode);
        //}

        //private Dictionary<string, object> GetPropertyValues(IContentBase content)
        //{
        //    var propertyEditorResolver = PropertyEditorResolver.Current; // should inject

        //    return content
        //        .Properties
        //        .Select(property =>
        //        {
        //            var e = propertyEditorResolver.GetByAlias(property.PropertyType.PropertyEditorAlias);
        //            var v = e == null
        //                ? property.Value
        //                : e.ValueEditor.ConvertDbToString(property, property.PropertyType, _serviceContext.DataTypeService);
        //            return new KeyValuePair<string, object>(property.Alias, v);
        //        })
        //        .ToDictionary(x => x.Key, x => x.Value);
        //}

        //private ContentData CreateContentData(IContent content)
        //{
        //    return new ContentData
        //    {
        //        Name = content.Name,
        //        Published = content.Published,
        //        Version = content.Version,
        //        VersionDate = content.UpdateDate,
        //        WriterId = content.WriterId,
        //        TemplateId = content.Template == null ? -1 : content.Template.Id,
        //        Properties = GetPropertyValues(content)
        //    };
        //}

        //private ContentNode CreateContentNode(IContent newest, IContent published)
        //{
        //    var contentType = _contentTypeCache.Get(PublishedItemType.Content, newest.ContentTypeId);

        //    var draftData = newest.Published
        //        ? null
        //        : CreateContentData(newest);

        //    var publishedData = newest.Published
        //        ? CreateContentData(newest)
        //        : (published == null ? null : CreateContentData(published));

        //    var contentNode = new ContentNode(newest.Id, contentType,
        //        newest.Level, newest.Path, newest.SortOrder,
        //        newest.ParentId, newest.CreateDate, newest.CreatorId,
        //        draftData, publishedData);

        //    return contentNode;
        //}

        private void LockAndLoadDomains()
        {
            using (_domainStore.GetWriter(_scopeProvider))
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.Domains);
                    LoadDomainsLocked();
                    scope.Complete();
                }
            }
        }

        private void LoadDomainsLocked()
        {
            var domains = _serviceContext.DomainService.GetAll(true);
            foreach (var domain in domains
                .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, CultureInfo.GetCultureInfo(x.LanguageIsoCode), x.IsWildcard)))
            {
                _domainStore.Set(domain.Id, domain);
            }
        }

        #endregion

        #region Handle Notifications

        // note: if the service is not ready, ie _isReady is false, then notifications are ignored

        // SetUmbracoVersionStep issues a DistributedCache.Instance.RefreshAll...() call which should cause
        // the entire content, media etc caches to reload from database -- and then the app restarts -- however,
        // at the time SetUmbracoVersionStep runs, Umbraco is not fully initialized and therefore some property
        // value converters, etc are not registered, and rebuilding the NuCache may not work properly.
        //
        // More details: ApplicationContext.IsConfigured being false, ApplicationEventHandler.ExecuteWhen... is
        // called and in most cases events are skipped, so property value converters are not registered or
        // removed, so PublishedPropertyType either initializes with the wrong converter, or throws because it
        // detects more than one converter for a property type.
        //
        // It's not an issue for XmlStore - the app restart takes place *after* the install has refreshed the
        // cache, and XmlStore just writes a new umbraco.config file upon RefreshAll, so that's OK.
        //
        // But for NuCache... we cannot rebuild the cache now. So it will NOT work and we are not fixing it,
        // because now we should ALWAYS run with the database server messenger, and then the RefreshAll will
        // be processed as soon as we are configured and the messenger processes instructions.

        public override void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged)
        {
            // no cache, nothing we can do
            if (_isReady == false)
            {
                draftChanged = publishedChanged = false;
                return;
            }

            using (_contentStore.GetWriter(_scopeProvider))
            {
                NotifyLocked(payloads, out bool draftChanged2, out bool publishedChanged2);
                draftChanged = draftChanged2;
                publishedChanged = publishedChanged2;
            }

            if (draftChanged || publishedChanged)
                ((PublishedSnapshot)CurrentPublishedSnapshot)?.Resync();
        }

        private void NotifyLocked(IEnumerable<ContentCacheRefresher.JsonPayload> payloads, out bool draftChanged, out bool publishedChanged)
        {
            publishedChanged = false;
            draftChanged = false;

            // locks:
            // content (and content types) are read-locked while reading content
            // contentStore is wlocked (so readable, only no new views)
            // and it can be wlocked by 1 thread only at a time
            // contentStore is write-locked during changes

            foreach (var payload in payloads)
            {
                _logger.Debug<PublishedSnapshotService>("Notified {ChangeTypes} for content {ContentId}", payload.ChangeTypes, payload.Id);

                if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    using (var scope = _scopeProvider.CreateScope())
                    {
                        scope.ReadLock(Constants.Locks.ContentTree);
                        LoadContentFromDatabaseLocked(scope);
                        scope.Complete();
                    }
                    draftChanged = publishedChanged = true;
                    continue;
                }

                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    if (_contentStore.Clear(payload.Id))
                        draftChanged = publishedChanged = true;
                    continue;
                }

                if (payload.ChangeTypes.HasTypesNone(TreeChangeTypes.RefreshNode | TreeChangeTypes.RefreshBranch))
                {
                    // ?!
                    continue;
                }

                // fixme - should we do some RV check here? (later)

                var capture = payload;
                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.ContentTree);

                    if (capture.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        // ?? should we do some RV check here?
                        var kits = _dataSource.GetBranchContentSources(scope, capture.Id);
                        _contentStore.SetBranch(capture.Id, kits);
                    }
                    else
                    {
                        // ?? should we do some RV check here?
                        var kit = _dataSource.GetContentSource(scope, capture.Id);
                        if (kit.IsEmpty)
                        {
                            _contentStore.Clear(capture.Id);
                        }
                        else
                        {
                            _contentStore.Set(kit);
                        }
                    }

                    scope.Complete();
                }

                // ?? cannot tell really because we're not doing RV checks
                draftChanged = publishedChanged = true;
            }
        }

        public override void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged)
        {
            // no cache, nothing we can do
            if (_isReady == false)
            {
                anythingChanged = false;
                return;
            }

            using (_mediaStore.GetWriter(_scopeProvider))
            {
                NotifyLocked(payloads, out bool anythingChanged2);
                anythingChanged = anythingChanged2;
            }

            if (anythingChanged)
                ((PublishedSnapshot)CurrentPublishedSnapshot)?.Resync();
        }

        private void NotifyLocked(IEnumerable<MediaCacheRefresher.JsonPayload> payloads, out bool anythingChanged)
        {
            anythingChanged = false;

            // locks:
            // see notes for content cache refresher

            foreach (var payload in payloads)
            {
                _logger.Debug<PublishedSnapshotService>("Notified {ChangeTypes} for media {MediaId}", payload.ChangeTypes, payload.Id);

                if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    using (var scope = _scopeProvider.CreateScope())
                    {
                        scope.ReadLock(Constants.Locks.MediaTree);
                        LoadMediaFromDatabaseLocked(scope);
                        scope.Complete();
                    }
                    anythingChanged = true;
                    continue;
                }

                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    if (_mediaStore.Clear(payload.Id))
                        anythingChanged = true;
                    continue;
                }

                if (payload.ChangeTypes.HasTypesNone(TreeChangeTypes.RefreshNode | TreeChangeTypes.RefreshBranch))
                {
                    // ?!
                    continue;
                }

                // fixme - should we do some RV checks here? (later)

                var capture = payload;
                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.MediaTree);

                    if (capture.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        // ?? should we do some RV check here?
                        var kits = _dataSource.GetBranchMediaSources(scope, capture.Id);
                        _mediaStore.SetBranch(capture.Id, kits);
                    }
                    else
                    {
                        // ?? should we do some RV check here?
                        var kit = _dataSource.GetMediaSource(scope, capture.Id);
                        if (kit.IsEmpty)
                        {
                            _mediaStore.Clear(capture.Id);
                        }
                        else
                        {
                            _mediaStore.Set(kit);
                        }
                    }

                    scope.Complete();
                }

                // ?? cannot tell really because we're not doing RV checks
                anythingChanged = true;
            }
        }

        public override void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads)
        {
            // no cache, nothing we can do
            if (_isReady == false)
                return;

            foreach (var payload in payloads)
                _logger.Debug<PublishedSnapshotService>("Notified {ChangeTypes} for {ItemType} {ItemId}", payload.ChangeTypes, payload.ItemType, payload.Id);

            Notify<IContentType>(_contentStore, payloads, RefreshContentTypesLocked);
            Notify<IMediaType>(_mediaStore, payloads, RefreshMediaTypesLocked);

            ((PublishedSnapshot)CurrentPublishedSnapshot)?.Resync();
        }

        private void Notify<T>(ContentStore store, ContentTypeCacheRefresher.JsonPayload[] payloads, Action<IEnumerable<int>, IEnumerable<int>, IEnumerable<int>, IEnumerable<int>> action)
        {
            var nameOfT = typeof(T).Name;

            var removedIds = new List<int>();
            var refreshedIds = new List<int>();
            var otherIds = new List<int>();
            var newIds = new List<int>();

            foreach (var payload in payloads)
            {
                if (payload.ItemType != nameOfT) continue;

                if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.Remove))
                    removedIds.Add(payload.Id);
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain))
                    refreshedIds.Add(payload.Id);
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshOther))
                    otherIds.Add(payload.Id);
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.Create))
                    newIds.Add(payload.Id);
            }

            if (removedIds.Count == 0 && refreshedIds.Count == 0 && otherIds.Count == 0 && newIds.Count == 0) return;

            using (store.GetWriter(_scopeProvider))
            {
                // ReSharper disable AccessToModifiedClosure
                action(removedIds, refreshedIds, otherIds, newIds);
                // ReSharper restore AccessToModifiedClosure
            }
        }

        public override void Notify(DataTypeCacheRefresher.JsonPayload[] payloads)
        {
            // no cache, nothing we can do
            if (_isReady == false)
                return;

            var idsA = payloads.Select(x => x.Id).ToArray();

            foreach (var payload in payloads)
                _logger.Debug<PublishedSnapshotService>("Notified {RemovedStatus} for data type {DataTypeId}",
                    payload.Removed ? "Removed" : "Refreshed",
                    payload.Id);

            using (_contentStore.GetWriter(_scopeProvider))
            using (_mediaStore.GetWriter(_scopeProvider))
            {
                // fixme - need to add a datatype lock
                // this is triggering datatypes reload in the factory, and right after we create some
                // content types by loading them ... there's a race condition here, which would require
                // some locking on datatypes
                _publishedContentTypeFactory.NotifyDataTypeChanges(idsA);

                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.ContentTree);
                    _contentStore.UpdateDataTypes(idsA, id => CreateContentType(PublishedItemType.Content, id));
                    scope.Complete();
                }

                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.MediaTree);
                    _mediaStore.UpdateDataTypes(idsA, id => CreateContentType(PublishedItemType.Media, id));
                    scope.Complete();
                }
            }

            ((PublishedSnapshot)CurrentPublishedSnapshot)?.Resync();
        }

        public override void Notify(DomainCacheRefresher.JsonPayload[] payloads)
        {
            // no cache, nothing we can do
            if (_isReady == false)
                return;

            using (_domainStore.GetWriter(_scopeProvider))
            {
                foreach (var payload in payloads)
                {
                    switch (payload.ChangeType)
                    {
                        case DomainChangeTypes.RefreshAll:
                            using (var scope = _scopeProvider.CreateScope())
                            {
                                scope.ReadLock(Constants.Locks.Domains);
                                LoadDomainsLocked();
                                scope.Complete();
                            }
                            break;
                        case DomainChangeTypes.Remove:
                            _domainStore.Clear(payload.Id);
                            break;
                        case DomainChangeTypes.Refresh:
                            var domain = _serviceContext.DomainService.GetById(payload.Id);
                            if (domain == null) continue;
                            if (domain.RootContentId.HasValue == false) continue; // anomaly
                            if (domain.LanguageIsoCode.IsNullOrWhiteSpace()) continue; // anomaly
                            var culture = CultureInfo.GetCultureInfo(domain.LanguageIsoCode);
                            _domainStore.Set(domain.Id, new Domain(domain.Id, domain.DomainName, domain.RootContentId.Value, culture, domain.IsWildcard));
                            break;
                    }
                }
            }
        }

        #endregion

        #region Content Types

        private IEnumerable<PublishedContentType> CreateContentTypes(PublishedItemType itemType, int[] ids)
        {
            // XxxTypeService.GetAll(empty) returns everything!
            if (ids.Length == 0)
                return Enumerable.Empty<PublishedContentType>();

            IEnumerable<IContentTypeComposition> contentTypes;
            switch (itemType)
            {
                case PublishedItemType.Content:
                    contentTypes = _serviceContext.ContentTypeService.GetAll(ids);
                    break;
                case PublishedItemType.Media:
                    contentTypes = _serviceContext.MediaTypeService.GetAll(ids);
                    break;
                case PublishedItemType.Member:
                    contentTypes = _serviceContext.MemberTypeService.GetAll(ids);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType));
            }

            // some may be missing - not checking here

            return contentTypes.Select(x => _publishedContentTypeFactory.CreateContentType(x));
        }

        private PublishedContentType CreateContentType(PublishedItemType itemType, int id)
        {
            IContentTypeComposition contentType;
            switch (itemType)
            {
                case PublishedItemType.Content:
                    contentType = _serviceContext.ContentTypeService.Get(id);
                    break;
                case PublishedItemType.Media:
                    contentType = _serviceContext.MediaTypeService.Get(id);
                    break;
                case PublishedItemType.Member:
                    contentType = _serviceContext.MemberTypeService.Get(id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType));
            }

            return contentType == null ? null : _publishedContentTypeFactory.CreateContentType(contentType);
        }

        private void RefreshContentTypesLocked(IEnumerable<int> removedIds, IEnumerable<int> refreshedIds, IEnumerable<int> otherIds, IEnumerable<int> newIds)
        {
            // locks:
            // content (and content types) are read-locked while reading content
            // contentStore is wlocked (so readable, only no new views)
            // and it can be wlocked by 1 thread only at a time

            var refreshedIdsA = refreshedIds.ToArray();

            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.ContentTypes);
                var typesA = CreateContentTypes(PublishedItemType.Content, refreshedIdsA).ToArray();
                var kits = _dataSource.GetTypeContentSources(scope, refreshedIdsA);
                _contentStore.UpdateContentTypes(removedIds, typesA, kits);
                _contentStore.UpdateContentTypes(CreateContentTypes(PublishedItemType.Content, otherIds.ToArray()).ToArray());
                _contentStore.NewContentTypes(CreateContentTypes(PublishedItemType.Content, newIds.ToArray()).ToArray());
                scope.Complete();
            }
        }

        private void RefreshMediaTypesLocked(IEnumerable<int> removedIds, IEnumerable<int> refreshedIds, IEnumerable<int> otherIds, IEnumerable<int> newIds)
        {
            // locks:
            // media (and content types) are read-locked while reading media
            // mediaStore is wlocked (so readable, only no new views)
            // and it can be wlocked by 1 thread only at a time

            var refreshedIdsA = refreshedIds.ToArray();

            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.MediaTypes);
                var typesA = CreateContentTypes(PublishedItemType.Media, refreshedIdsA).ToArray();
                var kits = _dataSource.GetTypeMediaSources(scope, refreshedIdsA);
                _mediaStore.UpdateContentTypes(removedIds, typesA, kits);
                _mediaStore.UpdateContentTypes(CreateContentTypes(PublishedItemType.Media, otherIds.ToArray()).ToArray());
                _mediaStore.NewContentTypes(CreateContentTypes(PublishedItemType.Media, newIds.ToArray()).ToArray());
                scope.Complete();
            }
        }

        #endregion

        #region Create, Get Published Snapshot

        private long _contentGen, _mediaGen, _domainGen;
        private IAppCache _elementsCache;

        public override IPublishedSnapshot CreatePublishedSnapshot(string previewToken)
        {
            // no cache, no joy
            if (_isReady == false)
                throw new InvalidOperationException("The published snapshot service has not properly initialized.");

            var preview = previewToken.IsNullOrWhiteSpace() == false;
            return new PublishedSnapshot(this, preview);
        }

        // gets a new set of elements
        // always creates a new set of elements,
        // even though the underlying elements may not change (store snapshots)
        public PublishedSnapshot.PublishedSnapshotElements GetElements(bool previewDefault)
        {
            // note: using ObjectCacheAppCache for elements and snapshot caches
            // is not recommended because it creates an inner MemoryCache which is a heavy
            // thing - better use a dictionary-based cache which "just" creates a concurrent
            // dictionary

            // for snapshot cache, DictionaryAppCache MAY be OK but it is not thread-safe,
            // nothing like that...
            // for elements cache, DictionaryAppCache is a No-No, use something better.
            // ie FastDictionaryAppCache (thread safe and all)

            ContentStore.Snapshot contentSnap, mediaSnap;
            SnapDictionary<int, Domain>.Snapshot domainSnap;
            IAppCache elementsCache;
            lock (_storesLock)
            {
                var scopeContext = _scopeProvider.Context;

                if (scopeContext == null)
                {
                    contentSnap = _contentStore.CreateSnapshot();
                    mediaSnap = _mediaStore.CreateSnapshot();
                    domainSnap = _domainStore.CreateSnapshot();
                    elementsCache = _elementsCache;
                }
                else
                {
                    contentSnap = _contentStore.LiveSnapshot;
                    mediaSnap = _mediaStore.LiveSnapshot;
                    domainSnap = _domainStore.Test.LiveSnapshot;
                    elementsCache = _elementsCache;

                    // this is tricky
                    // we are returning elements composed from live snapshots, which we need to replace
                    // with actual snapshots when the context is gone - but when the action runs, there
                    // still is a context - so we cannot get elements - just resync = nulls the current
                    // elements
                    // just need to make sure nothing gets elements in another enlisted action... so using
                    // a MaxValue to make sure this one runs last, and it should be ok
                    scopeContext.Enlist("Umbraco.Web.PublishedCache.NuCache.PublishedSnapshotService.Resync", () => this, (completed, svc) =>
                    {
                        ((PublishedSnapshot)svc.CurrentPublishedSnapshot)?.Resync();
                    }, int.MaxValue);
                }

                // create a new snapshot cache if snapshots are different gens
                if (contentSnap.Gen != _contentGen || mediaSnap.Gen != _mediaGen || domainSnap.Gen != _domainGen || _elementsCache == null)
                {
                    _contentGen = contentSnap.Gen;
                    _mediaGen = mediaSnap.Gen;
                    _domainGen = domainSnap.Gen;
                    elementsCache = _elementsCache = new FastDictionaryAppCache();
                }
            }

            var snapshotCache = new DictionaryAppCache();

            var memberTypeCache = new PublishedContentTypeCache(null, null, _serviceContext.MemberTypeService, _publishedContentTypeFactory, _logger);

            var defaultCulture = _defaultCultureAccessor.DefaultCulture;
            var domainCache = new DomainCache(domainSnap, defaultCulture);
            var domainHelper = new DomainHelper(domainCache, _siteDomainHelper);

            return new PublishedSnapshot.PublishedSnapshotElements
            {
                ContentCache = new ContentCache(previewDefault, contentSnap, snapshotCache, elementsCache, domainHelper, _globalSettings, _serviceContext.LocalizationService),
                MediaCache = new MediaCache(previewDefault, mediaSnap, snapshotCache, elementsCache),
                MemberCache = new MemberCache(previewDefault, snapshotCache, _serviceContext.MemberService, memberTypeCache, PublishedSnapshotAccessor, VariationContextAccessor, _entitySerializer),
                DomainCache = domainCache,
                SnapshotCache = snapshotCache,
                ElementsCache = elementsCache
            };
        }

        #endregion

        #region Preview

        public override string EnterPreview(IUser user, int contentId)
        {
            return "preview"; // anything
        }

        public override void RefreshPreview(string previewToken, int contentId)
        {
            // nothing
        }

        public override void ExitPreview(string previewToken)
        {
            // nothing
        }

        #endregion

        #region Handle Repository Events For Database PreCache

        // note: if the service is not ready, ie _isReady is false, then we still handle repository events,
        // because we can, we do not need a working published snapshot to do it - the only reason why it could cause an
        // issue is if the database table is not ready, but that should be prevented by migrations.

        // we need them to be "repository" events ie to trigger from within the repository transaction,
        // because they need to be consistent with the content that is being refreshed/removed - and that
        // should be guaranteed by a DB transaction

        private void OnContentRemovingEntity(DocumentRepository sender, DocumentRepository.ScopedEntityEventArgs args)
        {
            OnRemovedEntity(args.Scope.Database, args.Entity);
        }

        private void OnMediaRemovingEntity(MediaRepository sender, MediaRepository.ScopedEntityEventArgs args)
        {
            OnRemovedEntity(args.Scope.Database, args.Entity);
        }

        private void OnMemberRemovingEntity(MemberRepository sender, MemberRepository.ScopedEntityEventArgs args)
        {
            OnRemovedEntity(args.Scope.Database, args.Entity);
        }

        private void OnRemovedEntity(IUmbracoDatabase db, IContentBase item)
        {
            db.Execute("DELETE FROM cmsContentNu WHERE nodeId=@id", new { id = item.Id });
        }

        private void OnContentRefreshedEntity(DocumentRepository sender, DocumentRepository.ScopedEntityEventArgs args)
        {
            var db = args.Scope.Database;
            var content = (Content)args.Entity;

            // always refresh the edited data
            OnRepositoryRefreshed(db, content, false);

            // if unpublishing, remove published data from table
            if (content.PublishedState == PublishedState.Unpublishing)
                db.Execute("DELETE FROM cmsContentNu WHERE nodeId=@id AND published=1", new { id = content.Id });

            // if publishing, refresh the published data
            else if (content.PublishedState == PublishedState.Publishing)
                OnRepositoryRefreshed(db, content, true);
        }

        private void OnMediaRefreshedEntity(MediaRepository sender, MediaRepository.ScopedEntityEventArgs args)
        {
            var db = args.Scope.Database;
            var media = args.Entity;

            // refresh the edited data
            OnRepositoryRefreshed(db, media, false);
        }

        private void OnMemberRefreshedEntity(MemberRepository sender, MemberRepository.ScopedEntityEventArgs args)
        {
            var db = args.Scope.Database;
            var member = args.Entity;

            // refresh the edited data
            OnRepositoryRefreshed(db, member, true);
        }

        private void OnRepositoryRefreshed(IUmbracoDatabase db, IContentBase content, bool published)
        {
            // use a custom SQL to update row version on each update
            //db.InsertOrUpdate(dto);

            var dto = GetDto(content, published);
            db.InsertOrUpdate(dto,
                "SET data=@data, rv=rv+1 WHERE nodeId=@id AND published=@published",
                new
                {
                    data = dto.Data,
                    id = dto.NodeId,
                    published = dto.Published
                });
        }

        private void OnContentTypeRefreshedEntity(IContentTypeService sender, ContentTypeChange<IContentType>.EventArgs args)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var contentTypeIds = args.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (contentTypeIds.Any())
                RebuildContentDbCache(contentTypeIds: contentTypeIds);
        }

        private void OnMediaTypeRefreshedEntity(IMediaTypeService sender, ContentTypeChange<IMediaType>.EventArgs args)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var mediaTypeIds = args.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (mediaTypeIds.Any())
                RebuildMediaDbCache(contentTypeIds: mediaTypeIds);
        }

        private void OnMemberTypeRefreshedEntity(IMemberTypeService sender, ContentTypeChange<IMemberType>.EventArgs args)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var memberTypeIds = args.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (memberTypeIds.Any())
                RebuildMemberDbCache(contentTypeIds: memberTypeIds);
        }

        private ContentNuDto GetDto(IContentBase content, bool published)
        {
            // should inject these in ctor
            // BUT for the time being we decide not to support ConvertDbToXml/String
            //var propertyEditorResolver = PropertyEditorResolver.Current;
            //var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

            var propertyData = new Dictionary<string, PropertyData[]>();
            foreach (var prop in content.Properties)
            {
                var pdatas = new List<PropertyData>();
                foreach (var pvalue in prop.Values)
                {
                    // sanitize - properties should be ok but ... never knows
                    if (!prop.PropertyType.SupportsVariation(pvalue.Culture, pvalue.Segment))
                        continue;

                    // note: at service level, invariant is 'null', but here invariant becomes 'string.Empty'
                    var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
                    if (value != null)
                        pdatas.Add(new PropertyData { Culture = pvalue.Culture ?? string.Empty, Segment = pvalue.Segment ?? string.Empty, Value = value });

                    //Core.Composing.Current.Logger.Debug<PublishedSnapshotService>($"{content.Id} {prop.Alias} [{pvalue.LanguageId},{pvalue.Segment}] {value} {(published?"pub":"edit")}");

                    //if (value != null)
                    //{
                    //    var e = propertyEditorResolver.GetByAlias(prop.PropertyType.PropertyEditorAlias);

                    //    // We are converting to string, even for database values which are integer or
                    //    // DateTime, which is not optimum. Doing differently would require that we have a way to tell
                    //    // whether the conversion to XML string changes something or not... which we don't, and we
                    //    // don't want to implement it as PropertyValueEditor.ConvertDbToXml/String should die anyway.

                    //    // Don't think about improving the situation here: this is a corner case and the real
                    //    // thing to do is to get rig of PropertyValueEditor.ConvertDbToXml/String.

                    //    // Use ConvertDbToString to keep it simple, although everywhere we use ConvertDbToXml and
                    //    // nothing ensures that the two methods are consistent.

                    //    if (e != null)
                    //        value = e.ValueEditor.ConvertDbToString(prop, prop.PropertyType, dataTypeService);
                    //}
                }
                propertyData[prop.Alias] = pdatas.ToArray();
            }

            var cultureData = new Dictionary<string, CultureVariation>();

            // sanitize - names should be ok but ... never knows
            if (content.GetContentType().VariesByCulture())
            {
                var infos = content is IContent document
                    ? (published
                        ? document.PublishCultureInfos
                        : document.CultureInfos)
                    : content.CultureInfos;

                foreach (var (culture, info) in infos)
                {
                    var cultureIsDraft = !published && content is IContent d && d.IsCultureEdited(culture);
                    cultureData[culture] = new CultureVariation { Name = info.Name, Date = content.GetUpdateDate(culture) ?? DateTime.MinValue, IsDraft = cultureIsDraft };
                }
            }

            //the dictionary that will be serialized
            var nestedData = new ContentNestedData
            {
                PropertyData = propertyData,
                CultureData = cultureData
            };

            var dto = new ContentNuDto
            {
                NodeId = content.Id,
                Published = published,

                // note that numeric values (which are Int32) are serialized without their
                // type (eg "value":1234) and JsonConvert by default deserializes them as Int64

                Data = JsonConvert.SerializeObject(nestedData)
            };

            //Core.Composing.Current.Logger.Debug<PublishedSnapshotService>(dto.Data);

            return dto;
        }

        #endregion

        #region Rebuild Database PreCache

        public void RebuildContentDbCache(int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {
            using (var scope = _scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                RebuildContentDbCacheLocked(scope, groupSize, contentTypeIds);
                scope.Complete();
            }
        }

        // assumes content tree lock
        private void RebuildContentDbCacheLocked(IScope scope, int groupSize, IEnumerable<int> contentTypeIds)
        {
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var contentObjectType = Constants.ObjectTypes.Document;
            var db = scope.Database;

            // remove all - if anything fails the transaction will rollback
            if (contentTypeIds == null || contentTypeIdsA.Length == 0)
            {
                // must support SQL-CE
                db.Execute(@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                    new { objType = contentObjectType });
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                // must support SQL-CE
                db.Execute($@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
)",
                    new { objType = contentObjectType, ctypes = contentTypeIdsA });
            }

            // insert back - if anything fails the transaction will rollback
            var query = scope.SqlContext.Query<IContent>();
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                var descendants = _documentRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
                var items = new List<ContentNuDto>();
                foreach (var c in descendants)
                {
                    // always the edited version
                    items.Add(GetDto(c, false));
                    // and also the published version if it makes any sense
                    if (c.Published)
                        items.Add(GetDto(c, true));
                }

                db.BulkInsertRecords(items);
                processed += items.Count;
            } while (processed < total);
        }

        public void RebuildMediaDbCache(int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {
            using (var scope = _scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                scope.ReadLock(Constants.Locks.MediaTree);
                RebuildMediaDbCacheLocked(scope, groupSize, contentTypeIds);
                scope.Complete();
            }
        }

        // assumes media tree lock
        public void RebuildMediaDbCacheLocked(IScope scope, int groupSize, IEnumerable<int> contentTypeIds)
        {
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var mediaObjectType = Constants.ObjectTypes.Media;
            var db = scope.Database;

            // remove all - if anything fails the transaction will rollback
            if (contentTypeIds == null || contentTypeIdsA.Length == 0)
            {
                // must support SQL-CE
                db.Execute(@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                    new { objType = mediaObjectType });
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                // must support SQL-CE
                db.Execute($@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
)",
                    new { objType = mediaObjectType, ctypes = contentTypeIdsA });
            }

            // insert back - if anything fails the transaction will rollback
            var query = scope.SqlContext.Query<IMedia>();
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                var descendants = _mediaRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
                var items = descendants.Select(m => GetDto(m, false)).ToArray();
                db.BulkInsertRecords(items);
                processed += items.Length;
            } while (processed < total);
        }

        public void RebuildMemberDbCache(int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {
            using (var scope = _scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                RebuildMemberDbCacheLocked(scope, groupSize, contentTypeIds);
                scope.Complete();
            }
        }

        // assumes member tree lock
        public void RebuildMemberDbCacheLocked(IScope scope, int groupSize, IEnumerable<int> contentTypeIds)
        {
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var memberObjectType = Constants.ObjectTypes.Member;
            var db = scope.Database;

            // remove all - if anything fails the transaction will rollback
            if (contentTypeIds == null || contentTypeIdsA.Length == 0)
            {
                // must support SQL-CE
                db.Execute(@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                    new { objType = memberObjectType });
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                // must support SQL-CE
                db.Execute($@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
)",
                    new { objType = memberObjectType, ctypes = contentTypeIdsA });
            }

            // insert back - if anything fails the transaction will rollback
            var query = scope.SqlContext.Query<IMember>();
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                var descendants = _memberRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
                var items = descendants.Select(m => GetDto(m, false)).ToArray();
                db.BulkInsertRecords(items);
                processed += items.Length;
            } while (processed < total);
        }

        public bool VerifyContentDbCache()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var ok = VerifyContentDbCacheLocked(scope);
                scope.Complete();
                return ok;
            }
        }

        // assumes content tree lock
        private bool VerifyContentDbCacheLocked(IScope scope)
        {
            // every document should have a corresponding row for edited properties
            // and if published, may have a corresponding row for published properties

            var contentObjectType = Constants.ObjectTypes.Document;
            var db = scope.Database;

            var count = db.ExecuteScalar<int>($@"SELECT COUNT(*)
FROM umbracoNode
JOIN {Constants.DatabaseSchema.Tables.Document} ON umbracoNode.id={Constants.DatabaseSchema.Tables.Document}.nodeId
LEFT JOIN cmsContentNu nuEdited ON (umbracoNode.id=nuEdited.nodeId AND nuEdited.published=0)
LEFT JOIN cmsContentNu nuPublished ON (umbracoNode.id=nuPublished.nodeId AND nuPublished.published=1)
WHERE umbracoNode.nodeObjectType=@objType
AND nuEdited.nodeId IS NULL OR ({Constants.DatabaseSchema.Tables.Document}.published=1 AND nuPublished.nodeId IS NULL);"
                , new { objType = contentObjectType });

            return count == 0;
        }

        public bool VerifyMediaDbCache()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.MediaTree);
                var ok = VerifyMediaDbCacheLocked(scope);
                scope.Complete();
                return ok;
            }
        }

        // assumes media tree lock
        public bool VerifyMediaDbCacheLocked(IScope scope)
        {
            // every media item should have a corresponding row for edited properties

            var mediaObjectType = Constants.ObjectTypes.Media;
            var db = scope.Database;

            var count = db.ExecuteScalar<int>(@"SELECT COUNT(*)
FROM umbracoNode
LEFT JOIN cmsContentNu ON (umbracoNode.id=cmsContentNu.nodeId AND cmsContentNu.published=0)
WHERE umbracoNode.nodeObjectType=@objType
AND cmsContentNu.nodeId IS NULL
", new { objType = mediaObjectType });

            return count == 0;
        }

        public bool VerifyMemberDbCache()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var ok = VerifyMemberDbCacheLocked(scope);
                scope.Complete();
                return ok;
            }
        }

        // assumes member tree lock
        public bool VerifyMemberDbCacheLocked(IScope scope)
        {
            // every member item should have a corresponding row for edited properties

            var memberObjectType = Constants.ObjectTypes.Member;
            var db = scope.Database;

            var count = db.ExecuteScalar<int>(@"SELECT COUNT(*)
FROM umbracoNode
LEFT JOIN cmsContentNu ON (umbracoNode.id=cmsContentNu.nodeId AND cmsContentNu.published=0)
WHERE umbracoNode.nodeObjectType=@objType
AND cmsContentNu.nodeId IS NULL
", new { objType = memberObjectType });

            return count == 0;
        }

        #endregion

        #region Instrument

        public string GetStatus()
        {
            var dbCacheIsOk = VerifyContentDbCache()
                && VerifyMediaDbCache()
                && VerifyMemberDbCache();

            var cg = _contentStore.GenCount;
            var mg = _mediaStore.GenCount;
            var cs = _contentStore.SnapCount;
            var ms = _mediaStore.SnapCount;
            var ce = _contentStore.Count;
            var me = _mediaStore.Count;

            return "I'm feeling good, really." +
                " Database cache is " + (dbCacheIsOk ? "ok" : "NOT ok (rebuild?)") + "." +
                " ContentStore has " + cg + " generation" + (cg > 1 ? "s" : "") +
                ", " + cs + " snapshot" + (cs > 1 ? "s" : "") +
                " and " + ce + " entr" + (ce > 1 ? "ies" : "y") + "." +
                " MediaStore has " + mg + " generation" + (mg > 1 ? "s" : "") +
                ", " + ms + " snapshot" + (ms > 1 ? "s" : "") +
                " and " + me + " entr" + (me > 1 ? "ies" : "y") + ".";
        }

        public void Collect()
        {
            var contentCollect = _contentStore.CollectAsync();
            var mediaCollect = _mediaStore.CollectAsync();
            System.Threading.Tasks.Task.WaitAll(contentCollect, mediaCollect);
        }

        #endregion
    }
}
