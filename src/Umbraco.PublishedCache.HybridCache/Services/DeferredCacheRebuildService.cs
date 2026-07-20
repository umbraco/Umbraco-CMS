using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     Accumulates content type and media type IDs for deferred background cache rebuilds with de-duplication.
/// </summary>
/// <remarks>
///     Uses an <see cref="Interlocked" /> flag to ensure a single background worker processes pending IDs.
///     When IDs are queued while the worker is active, they accumulate and are picked up in the next
///     iteration. Only one Task.Run is scheduled at a time, avoiding thread-pool churn
///     under bursty saves.
/// </remarks>
internal sealed class DeferredCacheRebuildService : IDeferredCacheRebuildService, IDisposable
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly ILogger<DeferredCacheRebuildService> _logger;
    private readonly CancellationTokenSource _shutdownCts;
    private readonly ConcurrentDictionary<int, byte> _pendingContentTypeIds = new();
    private readonly ConcurrentDictionary<int, byte> _pendingMediaTypeIds = new();
    private int _processing; // 0 = idle, 1 = active

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeferredCacheRebuildService" /> class.
    /// </summary>
    /// <param name="documentCacheService">The document cache service used to rebuild content caches.</param>
    /// <param name="mediaCacheService">The media cache service used to rebuild media caches.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="hostApplicationLifetime">The application lifetime, used to cancel in-flight rebuilds on shutdown.</param>
    public DeferredCacheRebuildService(
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService,
        ILogger<DeferredCacheRebuildService> logger,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _logger = logger;
        _shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(hostApplicationLifetime.ApplicationStopping);
    }

    /// <inheritdoc />
    public void QueueContentTypeRebuild(IReadOnlyCollection<int> contentTypeIds)
    {
        foreach (var id in contentTypeIds)
        {
            _pendingContentTypeIds.TryAdd(id, 0);
        }

        ScheduleProcessing();
    }

    /// <inheritdoc />
    public void QueueMediaTypeRebuild(IReadOnlyCollection<int> mediaTypeIds)
    {
        foreach (var id in mediaTypeIds)
        {
            _pendingMediaTypeIds.TryAdd(id, 0);
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
                Task.Run(() => ProcessPendingRebuildsAsync(_shutdownCts.Token));
            }
        }
    }

    private async Task ProcessPendingRebuildsAsync(CancellationToken cancellationToken)
    {
        // Allow for retries in case of transient failures (e.g. brief database connectivity issues).
        // To avoid infinite retry loops in case of persistent failures, we limit the number of consecutive retries.
        const int MaxConsecutiveFailures = 3;
        var consecutiveFailures = 0;

        try
        {
            while (HasPendingIds())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var contentTypeIds = DrainIds(_pendingContentTypeIds);
                var mediaTypeIds = DrainIds(_pendingMediaTypeIds);

                try
                {
                    if (contentTypeIds.Length > 0)
                    {
                        _logger.LogInformation("Deferred rebuild starting for content type IDs: {ContentTypeIds}", contentTypeIds);
                        _documentCacheService.Rebuild(contentTypeIds);
                        await _documentCacheService.RebuildMemoryCacheByContentTypeAsync(contentTypeIds);
                        _logger.LogInformation("Deferred rebuild completed for content type IDs: {ContentTypeIds}", contentTypeIds);
                    }

                    if (mediaTypeIds.Length > 0)
                    {
                        _logger.LogInformation("Deferred rebuild starting for media type IDs: {MediaTypeIds}", mediaTypeIds);
                        _mediaCacheService.Rebuild(mediaTypeIds);
                        await _mediaCacheService.RebuildMemoryCacheByContentTypeAsync(mediaTypeIds);
                        _logger.LogInformation("Deferred rebuild completed for media type IDs: {MediaTypeIds}", mediaTypeIds);
                    }

                    consecutiveFailures = 0;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Shutdown requested — re-add IDs so no work is silently lost, then exit.
                    RequeueIds(_pendingContentTypeIds, contentTypeIds);
                    RequeueIds(_pendingMediaTypeIds, mediaTypeIds);
                    throw;
                }
                catch (Exception ex)
                {
                    consecutiveFailures++;

                    // Re-add failed IDs so they can be retried. Rebuild is idempotent, so
                    // re-queuing IDs that partially succeeded is safe.
                    RequeueIds(_pendingContentTypeIds, contentTypeIds);
                    RequeueIds(_pendingMediaTypeIds, mediaTypeIds);

                    if (consecutiveFailures >= MaxConsecutiveFailures)
                    {
                        _logger.LogError(
                            ex,
                            "Background cache rebuild failed {Count} consecutive times; IDs will be retried on the next content type save",
                            consecutiveFailures);
                        return;
                    }

                    _logger.LogWarning(
                        ex,
                        "Background cache rebuild failed (attempt {Attempt} of {Max}), retrying",
                        consecutiveFailures,
                        MaxConsecutiveFailures);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Background cache rebuild cancelled due to application shutdown");
        }
        finally
        {
            Interlocked.Exchange(ref _processing, 0);
        }

        // Re-check after clearing the flag. If IDs were added between the last
        // HasPendingIds() == false and the flag reset, the Queue* caller's
        // ScheduleProcessing would have seen _processing == 1 and skipped Task.Run,
        // leaving no worker scheduled. We pick them up here.
        if (HasPendingIds())
        {
            ScheduleProcessing();
        }
    }

    /// <summary>
    ///     Waits until the background worker is idle and no IDs are pending. Internal for test purposes.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>A task that completes when both the worker is idle and all pending IDs have been processed.</returns>
    internal async Task WaitForPendingRebuildsAsync(CancellationToken cancellationToken = default)
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
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>A task that completes when the worker is no longer active.</returns>
    internal async Task WaitForWorkerIdleAsync(CancellationToken cancellationToken = default)
    {
        while (Volatile.Read(ref _processing) == 1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(10, cancellationToken);
        }
    }

    private bool HasPendingIds() => _pendingContentTypeIds.IsEmpty is false || _pendingMediaTypeIds.IsEmpty is false;

    /// <summary>
    ///     Snapshots and removes all keys from the dictionary, returning them as an array.
    /// </summary>
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
