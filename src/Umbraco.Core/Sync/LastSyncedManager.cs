using System.ComponentModel;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Sync;

internal sealed class LastSyncedManager : ILastSyncedManager
{
    private readonly ILastSyncedRepository _lastSyncedRepository;
    private readonly IScopeProvider _scopeProvider;
    private readonly Lock _internalIdLock = new();
    private int? _lastSyncedInternalId;
    private int? _lastSyncedExternalId;

    public LastSyncedManager(ILastSyncedRepository lastSyncedRepository, IScopeProvider scopeProvider)
    {
        _lastSyncedRepository = lastSyncedRepository;
        _scopeProvider = scopeProvider;
    }

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

    public async Task<Attempt<LastSyncedOperationStatus>> SaveLastSyncedInternalAsync(int id)
    {
        if (id < 0)
        {
            return Attempt.Fail(LastSyncedOperationStatus.InvalidId);
        }

        await GetLastSyncedInternalAsync();
        RaiseInternalId(id);

        return Attempt.Succeed(LastSyncedOperationStatus.Success);
    }

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

    public async Task<Attempt<LastSyncedOperationStatus>> DeleteOlderThanAsync(DateTime date)
    {
        using ICoreScope scope = _scopeProvider.CreateScope();
        await _lastSyncedRepository.DeleteEntriesOlderThanAsync(date);
        scope.Complete();

        return Attempt.Succeed(LastSyncedOperationStatus.Success);
    }

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
