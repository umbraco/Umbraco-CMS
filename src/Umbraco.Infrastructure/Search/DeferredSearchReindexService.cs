using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.Search;

/// <summary>
///     Accumulates content type, media type, and member type IDs for deferred background search reindexing
///     with de-duplication.
/// </summary>
/// <remarks>
///     Uses an <see cref="Interlocked" /> flag to ensure a single background worker processes pending IDs.
///     When IDs are queued while the worker is active, they accumulate and are picked up in the next
///     iteration. Only one Task.Run is scheduled at a time, avoiding thread-pool churn
///     under bursty saves.
/// </remarks>
internal sealed class DeferredSearchReindexService : IDeferredSearchReindexService, IDisposable
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;
    private readonly IPublishStatusQueryService _publishStatusQueryService;
    private readonly IOptionsMonitor<IndexingSettings> _indexingSettings;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ILogger<DeferredSearchReindexService> _logger;
    private readonly CancellationTokenSource _shutdownCts;
    private readonly ConcurrentDictionary<int, byte> _pendingContentTypeIds = new();
    private readonly ConcurrentDictionary<int, byte> _pendingMediaTypeIds = new();
    private readonly ConcurrentDictionary<int, byte> _pendingMemberTypeIds = new();
    private int _processing; // 0 = idle, 1 = active

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeferredSearchReindexService" /> class.
    /// </summary>
    /// <param name="documentRepository">Repository used to page through content items without acquiring distributed locks.</param>
    /// <param name="mediaRepository">Repository used to page through media items without acquiring distributed locks.</param>
    /// <param name="memberRepository">Repository used to page through member items without acquiring distributed locks.</param>
    /// <param name="umbracoIndexingHandler">The handler responsible for writing entries to Examine indexes.</param>
    /// <param name="publishStatusQueryService">Service used to check whether content has a published ancestor path.</param>
    /// <param name="indexingSettings">The indexing configuration settings, providing the paging batch size.</param>
    /// <param name="scopeProvider">The scope provider, used to create scopes for repository access.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="hostApplicationLifetime">The application lifetime, used to cancel in-flight reindexing on shutdown.</param>
    public DeferredSearchReindexService(
        IDocumentRepository documentRepository,
        IMediaRepository mediaRepository,
        IMemberRepository memberRepository,
        IUmbracoIndexingHandler umbracoIndexingHandler,
        IPublishStatusQueryService publishStatusQueryService,
        IOptionsMonitor<IndexingSettings> indexingSettings,
        ICoreScopeProvider scopeProvider,
        ILogger<DeferredSearchReindexService> logger,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _documentRepository = documentRepository;
        _mediaRepository = mediaRepository;
        _memberRepository = memberRepository;
        _umbracoIndexingHandler = umbracoIndexingHandler;
        _publishStatusQueryService = publishStatusQueryService;
        _indexingSettings = indexingSettings;
        _scopeProvider = scopeProvider;
        _logger = logger;
        _shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(hostApplicationLifetime.ApplicationStopping);
    }

    /// <inheritdoc />
    public void QueueContentTypeReindex(IReadOnlyCollection<int> contentTypeIds)
    {
        foreach (var id in contentTypeIds)
        {
            _pendingContentTypeIds.TryAdd(id, 0);
        }

        ScheduleProcessing();
    }

    /// <inheritdoc />
    public void QueueMediaTypeReindex(IReadOnlyCollection<int> mediaTypeIds)
    {
        foreach (var id in mediaTypeIds)
        {
            _pendingMediaTypeIds.TryAdd(id, 0);
        }

        ScheduleProcessing();
    }

    /// <inheritdoc />
    public void QueueMemberTypeReindex(IReadOnlyCollection<int> memberTypeIds)
    {
        foreach (var id in memberTypeIds)
        {
            _pendingMemberTypeIds.TryAdd(id, 0);
        }

        ScheduleProcessing();
    }

    private void ScheduleProcessing()
    {
        if (_shutdownCts.IsCancellationRequested)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _processing, 1, 0) == 0)
        {
            // Suppress execution context flow so the background task does not inherit the
            // caller's ambient scope (stored in AsyncLocal). Without this, the background
            // thread would share the same database connection as the notification handler,
            // causing "open DataReader" errors from concurrent use of a single connection.
            using (ExecutionContext.SuppressFlow())
            {
                Task.Run(() => ProcessPendingReindexAsync(_shutdownCts.Token));
            }
        }
    }

    private async Task ProcessPendingReindexAsync(CancellationToken cancellationToken)
    {
        const int MaxConsecutiveFailures = 3;
        var consecutiveFailures = 0;

        try
        {
            while (HasPendingIds())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var contentTypeIds = DrainIds(_pendingContentTypeIds);
                var mediaTypeIds = DrainIds(_pendingMediaTypeIds);
                var memberTypeIds = DrainIds(_pendingMemberTypeIds);

                try
                {
                    if (contentTypeIds.Length > 0)
                    {
                        _logger.LogInformation("Deferred reindex starting for content type IDs: {ContentTypeIds}", contentTypeIds);
                        ReindexContentOfContentTypes(contentTypeIds);
                        _logger.LogInformation("Deferred reindex completed for content type IDs: {ContentTypeIds}", contentTypeIds);
                    }

                    if (mediaTypeIds.Length > 0)
                    {
                        _logger.LogInformation("Deferred reindex starting for media type IDs: {MediaTypeIds}", mediaTypeIds);
                        ReindexMediaOfMediaTypes(mediaTypeIds);
                        _logger.LogInformation("Deferred reindex completed for media type IDs: {MediaTypeIds}", mediaTypeIds);
                    }

                    if (memberTypeIds.Length > 0)
                    {
                        _logger.LogInformation("Deferred reindex starting for member type IDs: {MemberTypeIds}", memberTypeIds);
                        ReindexMemberOfMemberTypes(memberTypeIds);
                        _logger.LogInformation("Deferred reindex completed for member type IDs: {MemberTypeIds}", memberTypeIds);
                    }

                    consecutiveFailures = 0;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    RequeueIds(_pendingContentTypeIds, contentTypeIds);
                    RequeueIds(_pendingMediaTypeIds, mediaTypeIds);
                    RequeueIds(_pendingMemberTypeIds, memberTypeIds);
                    throw;
                }
                catch (Exception ex)
                {
                    consecutiveFailures++;

                    RequeueIds(_pendingContentTypeIds, contentTypeIds);
                    RequeueIds(_pendingMediaTypeIds, mediaTypeIds);
                    RequeueIds(_pendingMemberTypeIds, memberTypeIds);

                    if (consecutiveFailures >= MaxConsecutiveFailures)
                    {
                        _logger.LogError(
                            ex,
                            "Background search reindex failed {Count} consecutive times; IDs will be retried on the next content type save",
                            consecutiveFailures);
                        return;
                    }

                    _logger.LogWarning(
                        ex,
                        "Background search reindex failed (attempt {Attempt} of {Max}), retrying",
                        consecutiveFailures,
                        MaxConsecutiveFailures);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Background search reindex cancelled due to application shutdown");
        }
        finally
        {
            Interlocked.Exchange(ref _processing, 0);
        }

        if (HasPendingIds())
        {
            ScheduleProcessing();
        }
    }

    private void ReindexContentOfContentTypes(int[] contentTypeIds)
    {
        List<int> contentTypeIdsAsList = [.. contentTypeIds];
        var publishChecked = new Dictionary<int, bool>();

        PageAndReindex(
            _documentRepository,
            () => _scopeProvider.CreateQuery<IContent>()
                .Where(x => contentTypeIdsAsList.Contains(x.ContentTypeId)),
            Ordering.By("Path"),
            c =>
            {
                var isPublished = false;
                if (c.Published)
                {
                    if (!publishChecked.TryGetValue(c.ParentId, out isPublished))
                    {
                        isPublished = _publishStatusQueryService.HasPublishedAncestorPath(c.Key);
                        publishChecked[c.Id] = isPublished;
                    }
                }

                _umbracoIndexingHandler.ReIndexForContent(c, isPublished);
            });
    }

    private void ReindexMediaOfMediaTypes(int[] mediaTypeIds)
    {
        List<int> mediaTypeIdsAsList = [.. mediaTypeIds];

        PageAndReindex(
            _mediaRepository,
            () => _scopeProvider.CreateQuery<IMedia>()
                .Where(x => mediaTypeIdsAsList.Contains(x.ContentTypeId)),
            Ordering.By("Path"),
            c => _umbracoIndexingHandler.ReIndexForMedia(c, c.Trashed == false));
    }

    private void ReindexMemberOfMemberTypes(int[] memberTypeIds)
    {
        List<int> memberTypeIdsAsList = [.. memberTypeIds];

        PageAndReindex(
            _memberRepository,
            () => _scopeProvider.CreateQuery<IMember>()
                .Where(x => memberTypeIdsAsList.Contains(x.ContentTypeId)),
            Ordering.By("LoginName"),
            c => _umbracoIndexingHandler.ReIndexForMember(c));
    }

    /// <summary>
    ///     Pages through a repository without acquiring distributed locks and invokes an action for each item.
    /// </summary>
    /// <remarks>
    ///     We use repositories directly instead of service methods (e.g. <c>IContentService.GetPagedOfTypes</c>)
    ///     because the service acquires a distributed ReadLock on the umbracoLock table. That lock uses a
    ///     REPEATABLEREAD table hint, holding a shared lock on the row until the scope ends. A concurrent
    ///     content save needs a WriteLock on the same row (exclusive lock), causing a timeout.
    ///     Skipping the lock is safe because reindexing is idempotent and eventually consistent — individual
    ///     content saves already trigger their own reindex via the normal notification pipeline.
    /// </remarks>
    private void PageAndReindex<TEntity>(
        IContentRepository<int, TEntity> repository,
        Func<IQuery<TEntity>> queryFactory,
        Ordering ordering,
        Action<TEntity> reindex)
        where TEntity : class, IContentBase
    {
        var pageSize = _indexingSettings.CurrentValue.BatchSize;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            TEntity[] items;
            using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
            {
                items = repository.GetPage(
                    queryFactory(), page++, pageSize, out total, null, null, ordering).ToArray();
            }

            foreach (TEntity item in items)
            {
                reindex(item);
            }
        }
    }

    /// <summary>
    ///     Waits until the background worker is idle and no IDs are pending. Internal for test purposes.
    /// </summary>
    internal async Task WaitForPendingReindexAsync(CancellationToken cancellationToken = default)
    {
        while (Volatile.Read(ref _processing) == 1 || HasPendingIds())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(10, cancellationToken);
        }
    }

    /// <summary>
    ///     Waits until the background worker finishes, even if IDs remain pending (e.g. after max retries).
    ///     Internal for test purposes.
    /// </summary>
    internal async Task WaitForWorkerIdleAsync(CancellationToken cancellationToken = default)
    {
        while (Volatile.Read(ref _processing) == 1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(10, cancellationToken);
        }
    }

    private bool HasPendingIds() =>
        _pendingContentTypeIds.IsEmpty is false ||
        _pendingMediaTypeIds.IsEmpty is false ||
        _pendingMemberTypeIds.IsEmpty is false;

    private static int[] DrainIds(ConcurrentDictionary<int, byte> dictionary)
    {
        var keys = dictionary.Keys.ToArray();
        foreach (var key in keys)
        {
            dictionary.TryRemove(key, out _);
        }

        return keys;
    }

    private static void RequeueIds(ConcurrentDictionary<int, byte> dictionary, int[] ids)
    {
        foreach (var id in ids)
        {
            dictionary.TryAdd(id, 0);
        }
    }

    /// <inheritdoc />
    public void Dispose() => _shutdownCts.Dispose();
}
