using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     Accumulates content type and media type IDs for deferred background cache rebuilds with de-duplication.
/// </summary>
/// <remarks>
///     Uses a <see cref="SemaphoreSlim" /> to prevent concurrent rebuilds. When a rebuild is already in progress,
///     newly queued IDs accumulate and are processed in the next iteration after the current rebuild completes.
/// </remarks>
internal sealed class DeferredCacheRebuildService : IDeferredCacheRebuildService, IDisposable
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly ILogger<DeferredCacheRebuildService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentDictionary<int, byte> _pendingContentTypeIds = new();
    private readonly ConcurrentDictionary<int, byte> _pendingMediaTypeIds = new();

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

        Task.Run(ProcessPendingRebuildsAsync);
    }

    /// <inheritdoc />
    public void QueueMediaTypeRebuild(IReadOnlyCollection<int> mediaTypeIds)
    {
        foreach (var id in mediaTypeIds)
        {
            _pendingMediaTypeIds.TryAdd(id, 0);
        }

        Task.Run(ProcessPendingRebuildsAsync);
    }

    private async Task ProcessPendingRebuildsAsync()
    {
        if (_semaphore.Wait(0) is false)
        {
            // Another rebuild is already in progress; queued IDs will be picked up when it completes.
            return;
        }

        try
        {
            while (HasPendingIds())
            {
                var contentTypeIds = DrainIds(_pendingContentTypeIds);
                var mediaTypeIds = DrainIds(_pendingMediaTypeIds);

                if (contentTypeIds.Length > 0)
                {
                    _logger.LogInformation("Background rebuilding database cache for content type IDs: {ContentTypeIds}", contentTypeIds);
                    _documentCacheService.Rebuild(contentTypeIds);
                    await _documentCacheService.RebuildMemoryCacheByContentTypeAsync(contentTypeIds);
                    _logger.LogInformation("Background rebuild completed for content type IDs: {ContentTypeIds}", contentTypeIds);
                }

                if (mediaTypeIds.Length > 0)
                {
                    _logger.LogInformation("Background rebuilding database cache for media type IDs: {MediaTypeIds}", mediaTypeIds);
                    _mediaCacheService.Rebuild(mediaTypeIds);
                    await _mediaCacheService.RebuildMemoryCacheByContentTypeAsync(mediaTypeIds);
                    _logger.LogInformation("Background rebuild completed for media type IDs: {MediaTypeIds}", mediaTypeIds);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during background cache rebuild");
        }
        finally
        {
            _semaphore.Release();
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

    /// <inheritdoc />
    public void Dispose() => _semaphore.Dispose();
}
