using System.ComponentModel;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Sync;

/// <summary>
/// Default implementation of <see cref="ILastSyncedManager"/> that manages last synced IDs with caching.
/// </summary>
internal sealed class LastSyncedManager : ILastSyncedManager
{
    private readonly ILastSyncedRepository _lastSyncedRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
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
        if (_lastSyncedInternalId is not null)
        {
            return _lastSyncedInternalId;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        _lastSyncedInternalId = await _lastSyncedRepository.GetInternalIdAsync();
        scope.Complete();

        return _lastSyncedInternalId;
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

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        await _lastSyncedRepository.SaveInternalIdAsync(id);
        _lastSyncedInternalId = id;
        scope.Complete();
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
        _lastSyncedInternalId = null;
        _lastSyncedExternalId = null;
    }
}
