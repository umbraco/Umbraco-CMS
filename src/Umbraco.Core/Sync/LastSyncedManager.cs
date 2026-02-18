using System.ComponentModel;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Sync;

/// <summary>
/// Default implementation of <see cref="ILastSyncedManager"/> that manages last synced IDs with caching.
/// </summary>
internal sealed class LastSyncedManager : ILastSyncedManager
{
    private readonly ILastSyncedRepository _lastSyncedRepository;
    private readonly IScopeProvider _scopeProvider;
    private int? _lastSyncedInternalId;
    private int? _lastSyncedExternalId;

    /// <summary>
    /// Initializes a new instance of the <see cref="LastSyncedManager"/> class.
    /// </summary>
    /// <param name="lastSyncedRepository">The repository for persisting last synced data.</param>
    /// <param name="coreScopeProvider">The scope provider for database transactions.</param>
    public LastSyncedManager(ILastSyncedRepository lastSyncedRepository, IScopeProvider scopeProvider)
    {
        _lastSyncedRepository = lastSyncedRepository;
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc/>
    public async Task<int?> GetLastSyncedInternalAsync()
    {
        if (_lastSyncedInternalId is not null)
        {
            return _lastSyncedInternalId;
        }

        using ICoreScope scope = _scopeProvider.CreateScope();
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

        using ICoreScope scope = _scopeProvider.CreateScope();
        _lastSyncedExternalId = await _lastSyncedRepository.GetExternalIdAsync();
        scope.Complete();

        return _lastSyncedExternalId;
    }

    /// <inheritdoc/>
    public async Task<Attempt<LastSyncedOperationStatus>> SaveLastSyncedInternalAsync(int id)
    {
        if (id < 0)
        {
            return Attempt.Fail(LastSyncedOperationStatus.InvalidId);
        }

        using ICoreScope scope = _scopeProvider.CreateScope();
        await _lastSyncedRepository.SaveInternalIdAsync(id);
        _lastSyncedInternalId = id;
        scope.Complete();

        return Attempt.Succeed(LastSyncedOperationStatus.Success);
    }

    /// <inheritdoc/>
    public async Task<Attempt<LastSyncedOperationStatus>> SaveLastSyncedExternalAsync(int id)
    {
        if (id < 0)
        {
            return Attempt.Fail(LastSyncedOperationStatus.InvalidId);
        }

        using ICoreScope scope = _scopeProvider.CreateScope();
        await _lastSyncedRepository.SaveExternalIdAsync(id);
        _lastSyncedExternalId = id;
        scope.Complete();

        return Attempt.Succeed(LastSyncedOperationStatus.Success);
    }

    /// <inheritdoc/>
    public async Task<Attempt<LastSyncedOperationStatus>> DeleteOlderThanAsync(DateTime date)
    {
        using ICoreScope scope = _scopeProvider.CreateScope();
        await _lastSyncedRepository.DeleteEntriesOlderThanAsync(date);
        scope.Complete();

        return Attempt.Succeed(LastSyncedOperationStatus.Success);
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
