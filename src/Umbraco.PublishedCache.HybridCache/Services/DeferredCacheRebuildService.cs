using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     Accumulates content type and media type IDs for deferred background cache rebuilds with de-duplication.
/// </summary>
/// <remarks>
///     Uses an <see cref="Interlocked" /> flag to ensure a single background worker processes pending IDs.
///     When IDs are queued while the worker is active, they accumulate and are picked up in the next
///     iteration. Only one <see cref="Task.Run" /> is scheduled at a time, avoiding thread-pool churn
///     under bursty saves.
/// </remarks>
internal sealed class DeferredCacheRebuildService : IDeferredCacheRebuildService
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly ILogger<DeferredCacheRebuildService> _logger;
    private readonly ConcurrentDictionary<int, byte> _pendingContentTypeIds = new();
    private readonly ConcurrentDictionary<int, byte> _pendingMediaTypeIds = new();
    private int _processing; // 0 = idle, 1 = active

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeferredCacheRebuildService" /> class.
    /// </summary>
    /// <param name="documentCacheService">The document cache service used to rebuild content caches.</param>
    /// <param name="mediaCacheService">The media cache service used to rebuild media caches.</param>
    /// <param name="logger">The logger.</param>
    public DeferredCacheRebuildService(
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService,
        ILogger<DeferredCacheRebuildService> logger)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _logger = logger;
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
        if (Interlocked.CompareExchange(ref _processing, 1, 0) == 0)
        {
            Task.Run(ProcessPendingRebuildsAsync);
        }
    }

    private async Task ProcessPendingRebuildsAsync()
    {
        try
        {
            while (HasPendingIds())
            {
                var contentTypeIds = DrainIds(_pendingContentTypeIds);
                var mediaTypeIds = DrainIds(_pendingMediaTypeIds);

                if (contentTypeIds.Length > 0)
                {
                    _logger.LogDebug("Deferred rebuild starting for content type IDs: {ContentTypeIds}", contentTypeIds);
                    _documentCacheService.Rebuild(contentTypeIds);
                    await _documentCacheService.RebuildMemoryCacheByContentTypeAsync(contentTypeIds);
                    _logger.LogDebug("Deferred rebuild completed for content type IDs: {ContentTypeIds}", contentTypeIds);
                }

                if (mediaTypeIds.Length > 0)
                {
                    _logger.LogDebug("Deferred rebuild starting for media type IDs: {MediaTypeIds}", mediaTypeIds);
                    _mediaCacheService.Rebuild(mediaTypeIds);
                    await _mediaCacheService.RebuildMemoryCacheByContentTypeAsync(mediaTypeIds);
                    _logger.LogDebug("Deferred rebuild completed for media type IDs: {MediaTypeIds}", mediaTypeIds);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during background cache rebuild");
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

    // Internal for test purposes — waits until the worker is idle and no IDs are pending.
    internal async Task WaitForPendingRebuildsAsync(CancellationToken cancellationToken = default)
    {
        while (Volatile.Read(ref _processing) == 1 || HasPendingIds())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(10, cancellationToken);
        }
    }

    private bool HasPendingIds() => _pendingContentTypeIds.IsEmpty is false || _pendingMediaTypeIds.IsEmpty is false;

    /// <summary>
    ///     Atomically snapshots and removes all keys from the dictionary, returning them as an array.
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
}
