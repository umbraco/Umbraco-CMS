using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpTest.Net.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Infrastructure.PublishedCache.Persistence;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;
using File = System.IO.File;

namespace Umbraco.Cms.Infrastructure.PublishedCache
{
    internal class PublishedSnapshotService : IPublishedSnapshotService
    {
        private readonly ServiceContext _serviceContext;
        private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IProfilingLogger _profilingLogger;
        private readonly IScopeProvider _scopeProvider;
        private readonly INuCacheContentService _publishedContentService;
        private readonly ILogger<PublishedSnapshotService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly GlobalSettings _globalSettings;
        private readonly IEntityXmlSerializer _entitySerializer;
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly IDefaultCultureAccessor _defaultCultureAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly NuCacheSettings _config;

        private bool _isReady;
        private bool _isReadSet;
        private object _isReadyLock;

        private readonly ContentStore _contentStore;
        private readonly ContentStore _mediaStore;
        private readonly SnapDictionary<int, Domain> _domainStore;
        private readonly object _storesLock = new object();
        private readonly object _elementsLock = new object();

        private BPlusTree<int, ContentNodeKit> _localContentDb;
        private BPlusTree<int, ContentNodeKit> _localMediaDb;
        private bool _localContentDbExists;
        private bool _localMediaDbExists;

        private long _contentGen;
        private long _mediaGen;
        private long _domainGen;
        private IAppCache _elementsCache;

        // define constant - determines whether to use cache when previewing
        // to store eg routes, property converted values, anything - caching
        // means faster execution, but uses memory - not sure if we want it
        // so making it configurable.
        public static readonly bool FullCacheWhenPreviewing = true;

        public PublishedSnapshotService(
            PublishedSnapshotServiceOptions options,
            IMainDom mainDom,
            ServiceContext serviceContext,
            IPublishedContentTypeFactory publishedContentTypeFactory,
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IVariationContextAccessor variationContextAccessor,
            IProfilingLogger profilingLogger,
            ILoggerFactory loggerFactory,
            IScopeProvider scopeProvider,
            INuCacheContentService publishedContentService,
            IDefaultCultureAccessor defaultCultureAccessor,
            IOptions<GlobalSettings> globalSettings,
            IEntityXmlSerializer entitySerializer,
            IPublishedModelFactory publishedModelFactory,
            IHostingEnvironment hostingEnvironment,
            IOptions<NuCacheSettings> config)
        {
            _serviceContext = serviceContext;
            _publishedContentTypeFactory = publishedContentTypeFactory;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _variationContextAccessor = variationContextAccessor;
            _profilingLogger = profilingLogger;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<PublishedSnapshotService>();
            _scopeProvider = scopeProvider;
            _publishedContentService = publishedContentService;
            _defaultCultureAccessor = defaultCultureAccessor;
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _config = config.Value;

            // we need an Xml serializer here so that the member cache can support XPath,
            // for members this is done by navigating the serialized-to-xml member
            _entitySerializer = entitySerializer;
            _publishedModelFactory = publishedModelFactory;

            // lock this entire call, we only want a single thread to be accessing the stores at once and within
            // the call below to mainDom.Register, a callback may occur on a threadpool thread to MainDomRelease
            // at the same time as we are trying to write to the stores. MainDomRelease also locks on _storesLock so
            // it will not be able to close the stores until we are done populating (if the store is empty)
            lock (_storesLock)
            {
                if (!options.IgnoreLocalDb)
                {
                    mainDom.Register(MainDomRegister, MainDomRelease);

                    // stores are created with a db so they can write to it, but they do not read from it,
                    // stores need to be populated, happens in OnResolutionFrozen which uses _localDbExists to
                    // figure out whether it can read the databases or it should populate them from sql

                    _logger.LogInformation("Creating the content store, localContentDbExists? {LocalContentDbExists}", _localContentDbExists);
                    _contentStore = new ContentStore(_publishedSnapshotAccessor, _variationContextAccessor, _loggerFactory.CreateLogger("ContentStore"), _loggerFactory, _publishedModelFactory, _localContentDb);
                    _logger.LogInformation("Creating the media store, localMediaDbExists? {LocalMediaDbExists}", _localMediaDbExists);
                    _mediaStore = new ContentStore(_publishedSnapshotAccessor, _variationContextAccessor, _loggerFactory.CreateLogger("ContentStore"), _loggerFactory, _publishedModelFactory, _localMediaDb);
                }
                else
                {
                    _logger.LogInformation("Creating the content store (local db ignored)");
                    _contentStore = new ContentStore(_publishedSnapshotAccessor, _variationContextAccessor, _loggerFactory.CreateLogger("ContentStore"), _loggerFactory, _publishedModelFactory);
                    _logger.LogInformation("Creating the media store (local db ignored)");
                    _mediaStore = new ContentStore(_publishedSnapshotAccessor, _variationContextAccessor, _loggerFactory.CreateLogger("ContentStore"), _loggerFactory, _publishedModelFactory);
                }

                _domainStore = new SnapDictionary<int, Domain>();
            }
        }

        protected PublishedSnapshot CurrentPublishedSnapshot => (PublishedSnapshot)_publishedSnapshotAccessor.PublishedSnapshot;

        // NOTE: These aren't used within this object but are made available internally to improve the IdKey lookup performance
        // when nucache is enabled.
        // TODO: Does this need to be here?
        internal int GetDocumentId(Guid udi) => GetId(_contentStore, udi);

        internal int GetMediaId(Guid udi) => GetId(_mediaStore, udi);

        internal Guid GetDocumentUid(int id) => GetUid(_contentStore, id);

        internal Guid GetMediaUid(int id) => GetUid(_mediaStore, id);

        private int GetId(ContentStore store, Guid uid) => store.LiveSnapshot.Get(uid)?.Id ?? 0;

        private Guid GetUid(ContentStore store, int id) => store.LiveSnapshot.Get(id)?.Uid ?? Guid.Empty;

        /// <summary>
        /// Install phase of <see cref="IMainDom"/>
        /// </summary>
        /// <remarks>
        /// This is inside of a lock in MainDom so this is guaranteed to run if MainDom was acquired and guaranteed
        /// to not run if MainDom wasn't acquired.
        /// If MainDom was not acquired, then _localContentDb and _localMediaDb will remain null which means this appdomain
        /// will load in published content via the DB and in that case this appdomain will probably not exist long enough to
        /// serve more than a page of content.
        /// </remarks>
        private void MainDomRegister()
        {
            var path = GetLocalFilesPath();
            var localContentDbPath = Path.Combine(path, "NuCache.Content.db");
            var localMediaDbPath = Path.Combine(path, "NuCache.Media.db");

            _localContentDbExists = File.Exists(localContentDbPath);
            _localMediaDbExists = File.Exists(localMediaDbPath);

            // if both local databases exist then GetTree will open them, else new databases will be created
            _localContentDb = BTree.GetTree(localContentDbPath, _localContentDbExists, _config);
            _localMediaDb = BTree.GetTree(localMediaDbPath, _localMediaDbExists, _config);

            _logger.LogInformation("Registered with MainDom, localContentDbExists? {LocalContentDbExists}, localMediaDbExists? {LocalMediaDbExists}", _localContentDbExists, _localMediaDbExists);
        }

        /// <summary>
        /// Release phase of MainDom
        /// </summary>
        /// <remarks>
        /// This will execute on a threadpool thread
        /// </remarks>
        private void MainDomRelease()
        {
            _logger.LogDebug("Releasing from MainDom...");

            lock (_storesLock)
            {
                _logger.LogDebug("Releasing content store...");
                _contentStore?.ReleaseLocalDb(); // null check because we could shut down before being assigned
                _localContentDb = null;

                _logger.LogDebug("Releasing media store...");
                _mediaStore?.ReleaseLocalDb(); // null check because we could shut down before being assigned
                _localMediaDb = null;

                _logger.LogInformation("Released from MainDom");
            }
        }

        private string GetLocalFilesPath()
        {
            var path = Path.Combine(_hostingEnvironment.LocalTempPath, "NuCache");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        private void DeleteLocalFilesForContent()
        {
            if (Volatile.Read(ref _isReady) && _localContentDb != null)
            {
                throw new InvalidOperationException("Cannot delete local files while the cache uses them.");
            }

            var path = GetLocalFilesPath();
            var localContentDbPath = Path.Combine(path, "NuCache.Content.db");
            if (File.Exists(localContentDbPath))
            {
                File.Delete(localContentDbPath);
            }
        }

        private void DeleteLocalFilesForMedia()
        {
            if (Volatile.Read(ref _isReady) && _localMediaDb != null)
            {
                throw new InvalidOperationException("Cannot delete local files while the cache uses them.");
            }

            var path = GetLocalFilesPath();
            var localMediaDbPath = Path.Combine(path, "NuCache.Media.db");
            if (File.Exists(localMediaDbPath))
            {
                File.Delete(localMediaDbPath);
            }
        }

        /// <summary>
        /// Populates the stores
        /// </summary>
        internal void EnsureCaches() => LazyInitializer.EnsureInitialized(
            ref _isReady,
            ref _isReadSet,
            ref _isReadyLock,
            () =>
            {
                // even though we are ready locked here we want to ensure that the stores lock is also locked
                lock (_storesLock)
                {
                    var okContent = false;
                    var okMedia = false;

                    try
                    {
                        if (_localContentDbExists)
                        {
                            okContent = LockAndLoadContent(() => LoadContentFromLocalDbLocked(true));
                            if (!okContent)
                            {
                                _logger.LogWarning("Loading content from local db raised warnings, will reload from database.");
                            }
                        }

                        if (_localMediaDbExists)
                        {
                            okMedia = LockAndLoadMedia(() => LoadMediaFromLocalDbLocked(true));
                            if (!okMedia)
                            {
                                _logger.LogWarning("Loading media from local db raised warnings, will reload from database.");
                            }
                        }

                        if (!okContent)
                        {
                            LockAndLoadContent(() => LoadContentFromDatabaseLocked(true));
                        }

                        if (!okMedia)
                        {
                            LockAndLoadMedia(() => LoadMediaFromDatabaseLocked(true));
                        }

                        LockAndLoadDomains();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "Panic, exception while loading cache data.");
                        throw;
                    }

                    return true;
                }
            });

        // sudden panic... but in RepeatableRead can a content that I haven't already read, be removed
        // before I read it? NO! because the WHOLE content tree is read-locked using WithReadLocked.
        // don't panic.
        private bool LockAndLoadContent(Func<bool> action)
        {
            // first get a writer, then a scope
            // if there already is a scope, the writer will attach to it
            // otherwise, it will only exist here - cheap
            using (_contentStore.GetScopedWriteLock(_scopeProvider))
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var ok = action();
                scope.Complete();
                return ok;
            }
        }

        private bool LoadContentFromDatabaseLocked(bool onStartup)
        {
            // locks:
            // contentStore is wlocked (1 thread)
            // content (and types) are read-locked

            var contentTypes = _serviceContext.ContentTypeService.GetAll()
                .Select(x => _publishedContentTypeFactory.CreateContentType(x));

            _contentStore.SetAllContentTypesLocked(contentTypes);

            using (_profilingLogger.TraceDuration<PublishedSnapshotService>("Loading content from database"))
            {
                // beware! at that point the cache is inconsistent,
                // assuming we are going to SetAll content items!

                _localContentDb?.Clear();

                // IMPORTANT GetAllContentSources sorts kits by level + parentId + sortOrder
                var kits = _publishedContentService.GetAllContentSources();
                return onStartup ? _contentStore.SetAllFastSortedLocked(kits, true) : _contentStore.SetAllLocked(kits);
            }
        }

        private bool LoadContentFromLocalDbLocked(bool onStartup)
        {
            var contentTypes = _serviceContext.ContentTypeService.GetAll()
                    .Select(x => _publishedContentTypeFactory.CreateContentType(x));
            _contentStore.SetAllContentTypesLocked(contentTypes);

            using (_profilingLogger.TraceDuration<PublishedSnapshotService>("Loading content from local cache file"))
            {
                // beware! at that point the cache is inconsistent,
                // assuming we are going to SetAll content items!

                return LoadEntitiesFromLocalDbLocked(onStartup, _localContentDb, _contentStore, "content");
            }
        }

        private bool LockAndLoadMedia(Func<bool> action)
        {
            // see note in LockAndLoadContent
            using (_mediaStore.GetScopedWriteLock(_scopeProvider))
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.MediaTree);
                var ok = action();
                scope.Complete();
                return ok;
            }
        }

        private bool LoadMediaFromDatabaseLocked(bool onStartup)
        {
            // locks & notes: see content
            var mediaTypes = _serviceContext.MediaTypeService.GetAll()
                .Select(x => _publishedContentTypeFactory.CreateContentType(x));
            _mediaStore.SetAllContentTypesLocked(mediaTypes);

            using (_profilingLogger.TraceDuration<PublishedSnapshotService>("Loading media from database"))
            {
                // beware! at that point the cache is inconsistent,
                // assuming we are going to SetAll content items!
                _localMediaDb?.Clear();

                _logger.LogDebug("Loading media from database...");
                // IMPORTANT GetAllMediaSources sorts kits by level + parentId + sortOrder
                var kits = _publishedContentService.GetAllMediaSources();
                return onStartup ? _mediaStore.SetAllFastSortedLocked(kits, true) : _mediaStore.SetAllLocked(kits);
            }
        }

        private bool LoadMediaFromLocalDbLocked(bool onStartup)
        {
            var mediaTypes = _serviceContext.MediaTypeService.GetAll()
                    .Select(x => _publishedContentTypeFactory.CreateContentType(x));
            _mediaStore.SetAllContentTypesLocked(mediaTypes);

            using (_profilingLogger.TraceDuration<PublishedSnapshotService>("Loading media from local cache file"))
            {
                // beware! at that point the cache is inconsistent,
                // assuming we are going to SetAll content items!

                return LoadEntitiesFromLocalDbLocked(onStartup, _localMediaDb, _mediaStore, "media");
            }
        }

        private bool LoadEntitiesFromLocalDbLocked(bool onStartup, BPlusTree<int, ContentNodeKit> localDb, ContentStore store, string entityType)
        {
            var kits = localDb.Select(x => x.Value)
                    .OrderBy(x => x.Node.Level)
                    .ThenBy(x => x.Node.ParentContentId)
                    .ThenBy(x => x.Node.SortOrder) // IMPORTANT sort by level + parentId + sortOrder
                    .ToList();

            if (kits.Count == 0)
            {
                // If there's nothing in the local cache file, we should return false? YES even though the site legitately might be empty.
                // Is it possible that the cache file is empty but the database is not? YES... (well, it used to be possible)
                // * A new file is created when one doesn't exist, this will only be done when MainDom is acquired
                // * The new file will be populated as soon as LoadCachesOnStartup is called
                // * If the appdomain is going down the moment after MainDom was acquired and we've created an empty cache file,
                //      then the MainDom release callback is triggered from on a different thread, which will close the file and
                //      set the cache file reference to null. At this moment, it is possible that the file is closed and the
                //      reference is set to null BEFORE LoadCachesOnStartup which would mean that the current appdomain would load
                //      in the in-mem cache via DB calls, BUT this now means that there is an empty cache file which will be
                //      loaded by the next appdomain and it won't check if it's empty, it just assumes that since the cache
                //      file is there, that is correct.

                // Update: We will still return false here even though the above mentioned race condition has been fixed since we now
                // lock the entire operation of creating/populating the cache file with the same lock as releasing/closing the cache file

                _logger.LogInformation($"Tried to load {entityType} from the local cache file but it was empty.");
                return false;
            }

            return onStartup ? store.SetAllFastSortedLocked(kits, false) : store.SetAllLocked(kits);
        }

        private void LockAndLoadDomains()
        {
            // see note in LockAndLoadContent
            using (_domainStore.GetScopedWriteLock(_scopeProvider))
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.Domains);
                LoadDomainsLocked();
                scope.Complete();
            }
        }

        private void LoadDomainsLocked()
        {
            var domains = _serviceContext.DomainService.GetAll(true);
            foreach (var domain in domains
                .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, x.LanguageIsoCode, x.IsWildcard)))
            {
                _domainStore.SetLocked(domain.Id, domain);
            }
        }

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

        // note: notifications for content type and data type changes should be invoked with the
        // pure live model factory, if any, locked and refreshed - see ContentTypeCacheRefresher and
        // DataTypeCacheRefresher

        public void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged)
        {
            // no cache, trash everything
            if (Volatile.Read(ref _isReady) == false)
            {
                DeleteLocalFilesForContent();
                draftChanged = publishedChanged = true;
                return;
            }

            using (_contentStore.GetScopedWriteLock(_scopeProvider))
            {
                NotifyLocked(payloads, out bool draftChanged2, out bool publishedChanged2);
                draftChanged = draftChanged2;
                publishedChanged = publishedChanged2;
            }

            if (draftChanged || publishedChanged)
            {
                CurrentPublishedSnapshot?.Resync();
            }
        }

        // Calling this method means we have a lock on the contentStore (i.e. GetScopedWriteLock)
        private void NotifyLocked(IEnumerable<ContentCacheRefresher.JsonPayload> payloads, out bool draftChanged, out bool publishedChanged)
        {
            publishedChanged = false;
            draftChanged = false;

            // locks:
            // content (and content types) are read-locked while reading content
            // contentStore is wlocked (so readable, only no new views)
            // and it can be wlocked by 1 thread only at a time
            // contentStore is write-locked during changes - see note above, calls to this method are wrapped in contentStore.GetScopedWriteLock

            foreach (var payload in payloads)
            {
                _logger.LogDebug("Notified {ChangeTypes} for content {ContentId}", payload.ChangeTypes, payload.Id);

                if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    using (var scope = _scopeProvider.CreateScope())
                    {
                        scope.ReadLock(Constants.Locks.ContentTree);
                        LoadContentFromDatabaseLocked(false);
                        scope.Complete();
                    }
                    draftChanged = publishedChanged = true;
                    continue;
                }

                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    if (_contentStore.ClearLocked(payload.Id))
                        draftChanged = publishedChanged = true;
                    continue;
                }

                if (payload.ChangeTypes.HasTypesNone(TreeChangeTypes.RefreshNode | TreeChangeTypes.RefreshBranch))
                {
                    // ?!
                    continue;
                }

                // TODO: should we do some RV check here? (later)

                var capture = payload;
                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.ContentTree);

                    if (capture.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        // ?? should we do some RV check here?
                        // IMPORTANT GetbranchContentSources sorts kits by level and by sort order
                        var kits = _publishedContentService.GetBranchContentSources(capture.Id);
                        _contentStore.SetBranchLocked(capture.Id, kits);
                    }
                    else
                    {
                        // ?? should we do some RV check here?
                        var kit = _publishedContentService.GetContentSource(capture.Id);
                        if (kit.IsEmpty)
                        {
                            _contentStore.ClearLocked(capture.Id);
                        }
                        else
                        {
                            _contentStore.SetLocked(kit);
                        }
                    }

                    scope.Complete();
                }

                // ?? cannot tell really because we're not doing RV checks
                draftChanged = publishedChanged = true;
            }
        }

        /// <inheritdoc />
        public void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged)
        {
            // no cache, trash everything
            if (Volatile.Read(ref _isReady) == false)
            {
                DeleteLocalFilesForMedia();
                anythingChanged = true;
                return;
            }

            using (_mediaStore.GetScopedWriteLock(_scopeProvider))
            {
                NotifyLocked(payloads, out bool anythingChanged2);
                anythingChanged = anythingChanged2;
            }

            if (anythingChanged)
            {
                CurrentPublishedSnapshot?.Resync();
            }
        }

        private void NotifyLocked(IEnumerable<MediaCacheRefresher.JsonPayload> payloads, out bool anythingChanged)
        {
            anythingChanged = false;

            // locks:
            // see notes for content cache refresher

            foreach (var payload in payloads)
            {
                _logger.LogDebug("Notified {ChangeTypes} for media {MediaId}", payload.ChangeTypes, payload.Id);

                if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    using (var scope = _scopeProvider.CreateScope())
                    {
                        scope.ReadLock(Constants.Locks.MediaTree);
                        LoadMediaFromDatabaseLocked(false);
                        scope.Complete();
                    }

                    anythingChanged = true;
                    continue;
                }

                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    if (_mediaStore.ClearLocked(payload.Id))
                    {
                        anythingChanged = true;
                    }

                    continue;
                }

                if (payload.ChangeTypes.HasTypesNone(TreeChangeTypes.RefreshNode | TreeChangeTypes.RefreshBranch))
                {
                    // ?!
                    continue;
                }

                // TODO: should we do some RV checks here? (later)
                var capture = payload;
                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.MediaTree);

                    if (capture.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        // ?? should we do some RV check here?
                        // IMPORTANT GetbranchContentSources sorts kits by level and by sort order
                        var kits = _publishedContentService.GetBranchMediaSources(capture.Id);
                        _mediaStore.SetBranchLocked(capture.Id, kits);
                    }
                    else
                    {
                        // ?? should we do some RV check here?
                        var kit = _publishedContentService.GetMediaSource(capture.Id);
                        if (kit.IsEmpty)
                        {
                            _mediaStore.ClearLocked(capture.Id);
                        }
                        else
                        {
                            _mediaStore.SetLocked(kit);
                        }
                    }

                    scope.Complete();
                }

                // ?? cannot tell really because we're not doing RV checks
                anythingChanged = true;
            }
        }

        /// <inheritdoc />
        public void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads)
        {
            // no cache, nothing we can do
            if (Volatile.Read(ref _isReady) == false)
            {
                return;
            }

            foreach (var payload in payloads)
            {
                _logger.LogDebug("Notified {ChangeTypes} for {ItemType} {ItemId}", payload.ChangeTypes, payload.ItemType, payload.Id);
            }

            Notify<IContentType>(_contentStore, payloads, RefreshContentTypesLocked);
            Notify<IMediaType>(_mediaStore, payloads, RefreshMediaTypesLocked);

            if (_publishedModelFactory.IsLiveFactoryEnabled())
            {
                // In the case of Pure Live - we actually need to refresh all of the content and the media
                // see https://github.com/umbraco/Umbraco-CMS/issues/5671
                // The underlying issue is that in Pure Live the ILivePublishedModelFactory will re-compile all of the classes/models
                // into a new DLL for the application which includes both content types and media types.
                // Since the models in the cache are based on these actual classes, all of the objects in the cache need to be updated
                // to use the newest version of the class.

                // NOTE: Ideally this can be run on background threads here which would prevent blocking the UI
                // as is the case when saving a content type. Intially one would think that it won't be any different
                // between running this here or in another background thread immediately after with regards to how the
                // UI will respond because we already know between calling `WithSafeLiveFactoryReset` to reset the PureLive models
                // and this code here, that many front-end requests could be attempted to be processed. If that is the case, those pages are going to get a
                // model binding error and our ModelBindingExceptionFilter is going to to its magic to reload those pages so the end user is none the wiser.
                // So whether or not this executes 'here' or on a background thread immediately wouldn't seem to make any difference except that we can return
                // execution to the UI sooner.
                // BUT!... there is a difference IIRC. There is still execution logic that continues after this call on this thread with the cache refreshers
                // and those cache refreshers need to have the up-to-date data since other user cache refreshers will be expecting the data to be 'live'. If
                // we ran this on a background thread then those cache refreshers are going to not get 'live' data when they query the content cache which
                // they require.

                using (_contentStore.GetScopedWriteLock(_scopeProvider))
                {
                    NotifyLocked(new[] { new ContentCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) }, out _, out _);
                }

                using (_mediaStore.GetScopedWriteLock(_scopeProvider))
                {
                    NotifyLocked(new[] { new MediaCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) }, out _);
                }
            }

            CurrentPublishedSnapshot?.Resync();
        }

        private void Notify<T>(ContentStore store, ContentTypeCacheRefresher.JsonPayload[] payloads, Action<List<int>, List<int>, List<int>, List<int>> action)
            where T : IContentTypeComposition
        {
            if (payloads.Length == 0)
            {
                return; // nothing to do
            }

            var nameOfT = typeof(T).Name;

            List<int> removedIds = null, refreshedIds = null, otherIds = null, newIds = null;

            foreach (var payload in payloads)
            {
                if (payload.ItemType != nameOfT)
                {
                    continue;
                }

                if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.Remove))
                {
                    AddToList(ref removedIds, payload.Id);
                }
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain))
                {
                    AddToList(ref refreshedIds, payload.Id);
                }
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshOther))
                {
                    AddToList(ref otherIds, payload.Id);
                }
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.Create))
                {
                    AddToList(ref newIds, payload.Id);
                }
            }

            if (removedIds.IsCollectionEmpty() && refreshedIds.IsCollectionEmpty() && otherIds.IsCollectionEmpty() && newIds.IsCollectionEmpty())
            {
                return;
            }

            using (store.GetScopedWriteLock(_scopeProvider))
            {
                action(removedIds, refreshedIds, otherIds, newIds);
            }
        }

        public void Notify(DataTypeCacheRefresher.JsonPayload[] payloads)
        {
            // no cache, nothing we can do
            if (Volatile.Read(ref _isReady) == false)
            {
                return;
            }

            var idsA = payloads.Select(x => x.Id).ToArray();

            foreach (var payload in payloads)
            {
                _logger.LogDebug("Notified {RemovedStatus} for data type {DataTypeId}",
                    payload.Removed ? "Removed" : "Refreshed",
                    payload.Id);
            }

            using (_contentStore.GetScopedWriteLock(_scopeProvider))
            using (_mediaStore.GetScopedWriteLock(_scopeProvider))
            {
                // TODO: need to add a datatype lock
                // this is triggering datatypes reload in the factory, and right after we create some
                // content types by loading them ... there's a race condition here, which would require
                // some locking on datatypes
                _publishedContentTypeFactory.NotifyDataTypeChanges(idsA);

                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.ContentTree);
                    _contentStore.UpdateDataTypesLocked(idsA, id => CreateContentType(PublishedItemType.Content, id));
                    scope.Complete();
                }

                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.ReadLock(Constants.Locks.MediaTree);
                    _mediaStore.UpdateDataTypesLocked(idsA, id => CreateContentType(PublishedItemType.Media, id));
                    scope.Complete();
                }
            }

            CurrentPublishedSnapshot?.Resync();
        }

        public void Notify(DomainCacheRefresher.JsonPayload[] payloads)
        {
            // no cache, nothing we can do
            if (Volatile.Read(ref _isReady) == false)
            {
                return;
            }

            // see note in LockAndLoadContent
            using (_domainStore.GetScopedWriteLock(_scopeProvider))
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
                            _domainStore.ClearLocked(payload.Id);
                            break;
                        case DomainChangeTypes.Refresh:
                            var domain = _serviceContext.DomainService.GetById(payload.Id);
                            if (domain == null) continue;
                            if (domain.RootContentId.HasValue == false) continue; // anomaly
                            if (domain.LanguageIsoCode.IsNullOrWhiteSpace()) continue; // anomaly
                            var culture = domain.LanguageIsoCode;
                            _domainStore.SetLocked(domain.Id, new Domain(domain.Id, domain.DomainName, domain.RootContentId.Value, culture, domain.IsWildcard));
                            break;
                    }
                }
            }
        }

        // Methods used to prevent allocations of lists
        private void AddToList(ref List<int> list, int val) => GetOrCreateList(ref list).Add(val);

        private List<int> GetOrCreateList(ref List<int> list) => list ?? (list = new List<int>());

        private IReadOnlyCollection<IPublishedContentType> CreateContentTypes(PublishedItemType itemType, int[] ids)
        {
            // XxxTypeService.GetAll(empty) returns everything!
            if (ids.Length == 0)
            {
                return Array.Empty<IPublishedContentType>();
            }

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
            return contentTypes.Select(x => _publishedContentTypeFactory.CreateContentType(x)).ToList();
        }

        private IPublishedContentType CreateContentType(PublishedItemType itemType, int id)
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

        private void RefreshContentTypesLocked(List<int> removedIds, List<int> refreshedIds, List<int> otherIds, List<int> newIds)
        {
            if (removedIds.IsCollectionEmpty() && refreshedIds.IsCollectionEmpty() && otherIds.IsCollectionEmpty() && newIds.IsCollectionEmpty())
            {
                return;
            }

            // locks:
            // content (and content types) are read-locked while reading content
            // contentStore is wlocked (so readable, only no new views)
            // and it can be wlocked by 1 thread only at a time
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.ContentTypes);

                var typesA = refreshedIds.IsCollectionEmpty()
                    ? Array.Empty<IPublishedContentType>()
                    : CreateContentTypes(PublishedItemType.Content, refreshedIds.ToArray()).ToArray();

                var kits = refreshedIds.IsCollectionEmpty()
                    ? Array.Empty<ContentNodeKit>()
                    : _publishedContentService.GetTypeContentSources(refreshedIds).ToArray();

                _contentStore.UpdateContentTypesLocked(removedIds, typesA, kits);
                if (!otherIds.IsCollectionEmpty())
                {
                    _contentStore.UpdateContentTypesLocked(CreateContentTypes(PublishedItemType.Content, otherIds.ToArray()));
                }

                if (!newIds.IsCollectionEmpty())
                {
                    _contentStore.NewContentTypesLocked(CreateContentTypes(PublishedItemType.Content, newIds.ToArray()));
                }

                scope.Complete();
            }
        }

        private void RefreshMediaTypesLocked(List<int> removedIds, List<int> refreshedIds, List<int> otherIds, List<int> newIds)
        {
            if (removedIds.IsCollectionEmpty() && refreshedIds.IsCollectionEmpty() && otherIds.IsCollectionEmpty() && newIds.IsCollectionEmpty())
            {
                return;
            }

            // locks:
            // media (and content types) are read-locked while reading media
            // mediaStore is wlocked (so readable, only no new views)
            // and it can be wlocked by 1 thread only at a time
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.MediaTypes);

                var typesA = refreshedIds == null
                    ? Array.Empty<IPublishedContentType>()
                    : CreateContentTypes(PublishedItemType.Media, refreshedIds.ToArray()).ToArray();

                var kits = refreshedIds == null
                    ? Array.Empty<ContentNodeKit>()
                    : _publishedContentService.GetTypeMediaSources(refreshedIds).ToArray();

                _mediaStore.UpdateContentTypesLocked(removedIds, typesA, kits);
                if (!otherIds.IsCollectionEmpty())
                {
                    _mediaStore.UpdateContentTypesLocked(CreateContentTypes(PublishedItemType.Media, otherIds.ToArray()).ToArray());
                }

                if (!newIds.IsCollectionEmpty())
                {
                    _mediaStore.NewContentTypesLocked(CreateContentTypes(PublishedItemType.Media, newIds.ToArray()).ToArray());
                }

                scope.Complete();
            }
        }

        public IPublishedSnapshot CreatePublishedSnapshot(string previewToken)
        {
            EnsureCaches();

            // no cache, no joy
            if (Volatile.Read(ref _isReady) == false)
            {
                throw new InvalidOperationException("The published snapshot service has not properly initialized.");
            }

            var preview = previewToken.IsNullOrWhiteSpace() == false;
            return new PublishedSnapshot(this, preview);
        }

        // gets a new set of elements
        // always creates a new set of elements,
        // even though the underlying elements may not change (store snapshots)
        public PublishedSnapshot.PublishedSnapshotElements GetElements(bool previewDefault)
        {
            EnsureCaches();

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

            // Here we are reading/writing to shared objects so we need to lock (can't be _storesLock which manages the actual nucache files
            // and would result in a deadlock). Even though we are locking around underlying readlocks (within CreateSnapshot) it's because
            // we need to ensure that the result of contentSnap.Gen (etc) and the re-assignment of these values and _elements cache
            // are done atomically.

            lock (_elementsLock)
            {
                IScopeContext scopeContext = _scopeProvider.Context;

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
                        svc.CurrentPublishedSnapshot?.Resync();
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

            var memberTypeCache = new PublishedContentTypeCache(null, null, _serviceContext.MemberTypeService, _publishedContentTypeFactory, _loggerFactory.CreateLogger<PublishedContentTypeCache>());

            var defaultCulture = _defaultCultureAccessor.DefaultCulture;
            var domainCache = new DomainCache(domainSnap, defaultCulture);

            return new PublishedSnapshot.PublishedSnapshotElements
            {
                ContentCache = new ContentCache(previewDefault, contentSnap, snapshotCache, elementsCache, domainCache, Options.Create(_globalSettings), _variationContextAccessor),
                MediaCache = new MediaCache(previewDefault, mediaSnap, _variationContextAccessor),
                MemberCache = new MemberCache(previewDefault, memberTypeCache, _publishedSnapshotAccessor, _variationContextAccessor, _publishedModelFactory),
                DomainCache = domainCache,
                SnapshotCache = snapshotCache,
                ElementsCache = elementsCache
            };
        }

        /// <inheritdoc />
        public void Rebuild(
            int groupSize = 5000,
            IReadOnlyCollection<int> contentTypeIds = null,
            IReadOnlyCollection<int> mediaTypeIds = null,
            IReadOnlyCollection<int> memberTypeIds = null)
            => _publishedContentService.Rebuild(groupSize, contentTypeIds, mediaTypeIds, memberTypeIds);

        public async Task CollectAsync()
        {
            EnsureCaches();

            await _contentStore.CollectAsync();
            await _mediaStore.CollectAsync();
        }

        internal ContentStore GetContentStore()
        {
            EnsureCaches();
            return _contentStore;
        }

        internal ContentStore GetMediaStore()
        {
            EnsureCaches();
            return _mediaStore;
        }

        /// <inheritdoc/>
        public void Dispose()
        { }
    }
}
