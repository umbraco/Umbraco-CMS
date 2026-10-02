using System.ComponentModel;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Sync;

/// <summary>
/// Default implementation of <see cref="ILastSyncedManager"/> that manages last synced IDs with caching.
/// </summary>
/// <remarks>
/// The external id is persisted. The internal id is kept in memory only: it is recorded by the periodic sync and by
/// inline syncs on request threads, and writing it would join the caller's transaction and hold this server's
/// umbracoLastSynced row until that transaction commits. Before any internal id has been recorded, it starts at the
/// persisted external id, which the periodic sync only moves past instructions it has fully processed.
/// </remarks>
internal sealed class LastSyncedManager : ILastSyncedManager
{
    private readonly ILastSyncedRepository _lastSyncedRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly Lock _internalIdLock = new();
    private int? _lastSyncedInternalId;
    private int? _lastSyncedExternalId;

    /// <summary>
    /// Initializes a new instance of the <see cref="LastSyncedManager"/> class.
    /// </summary>
    /// <param name="lastSyncedRepository">The repository for persisting last synced data.</param>
    /// <param name="coreScopeProvider">The scope provider for database transactions.</param>
    public LastSyncedManager(ILastSyncedRepository lastSyncedRepository, ICoreScopeProvider coreScopeProvider)
    {
        _lastSyncedRepository = lastSyncedRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    /// <inheritdoc/>
    public async Task<int?> GetLastSyncedInternalAsync()
    {
        lock (_internalIdLock)
        {
            if (_lastSyncedInternalId is not null)
            {
                return _lastSyncedInternalId;
            }
        }

        int? persistedExternalId = await GetLastSyncedExternalAsync();

        lock (_internalIdLock)
        {
            _lastSyncedInternalId ??= persistedExternalId;
            return _lastSyncedInternalId;
        }
    }

    /// <inheritdoc/>
    public async Task<int?> GetLastSyncedExternalAsync()
    {
        if (_lastSyncedExternalId is not null)
        {
            return _lastSyncedExternalId;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        _lastSyncedExternalId = await _lastSyncedRepository.GetExternalIdAsync();
        scope.Complete();

        return _lastSyncedExternalId;
    }

    /// <inheritdoc/>
    public async Task SaveLastSyncedInternalAsync(int id)
    {
        if (id < 0)
        {
            throw new ArgumentException("Invalid last synced id. Must be non-negative.");
        }

        await GetLastSyncedInternalAsync();
        RaiseInternalId(id);
    }

    /// <inheritdoc/>
    public async Task SaveLastSyncedExternalAsync(int id)
    {
        if (id < 0)
        {
            throw new ArgumentException("Invalid last synced id. Must be non-negative.");
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        await _lastSyncedRepository.SaveExternalIdAsync(id);
        _lastSyncedExternalId = id;
        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task DeleteOlderThanAsync(DateTime date)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        await _lastSyncedRepository.DeleteEntriesOlderThanAsync(date);
        scope.Complete();
    }

    /// <summary>
    /// Clears the local cache of last synced IDs.
    /// </summary>
    /// <remarks>
    /// This method is intended for testing purposes only.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void ClearLocalCache()
    {
        lock (_internalIdLock)
        {
            _lastSyncedInternalId = null;
        }

        _lastSyncedExternalId = null;
    }

    // The periodic sync and inline syncs can record the same instructions concurrently or in a different order;
    // only ever moving the id forward keeps the checkpoint consistent.
    private void RaiseInternalId(int id)
    {
        lock (_internalIdLock)
        {
            if (_lastSyncedInternalId is null || _lastSyncedInternalId < id)
            {
                _lastSyncedInternalId = id;
            }
        }
    }
}
