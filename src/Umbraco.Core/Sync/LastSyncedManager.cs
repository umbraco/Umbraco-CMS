using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Sync;

/// <inheritdoc/>
internal sealed class LastSyncedManager : ILastSyncedManager
{
    private readonly ILastSyncedRepository _lastSyncedRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public LastSyncedManager(ILastSyncedRepository lastSyncedRepository, ICoreScopeProvider coreScopeProvider)
    {
        _lastSyncedRepository = lastSyncedRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    /// <inheritdoc/>
    public async Task<int?> GetLastSyncedInternalAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        int? internalId = await _lastSyncedRepository.GetInternalIdAsync();
        scope.Complete();

        return internalId;
    }

    /// <inheritdoc/>
    public async Task<int?> GetLastSyncedExternalAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        int? externalId = await _lastSyncedRepository.GetExternalIdAsync();
        scope.Complete();

        return externalId;
    }

    /// <inheritdoc/>
    public async Task SaveLastSyncedInternalAsync(int id)
    {
        if (id >= 0)
        {
            using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            await _lastSyncedRepository.SaveInternalIdAsync(id);
            scope.Complete();
        }
        else
        {
            throw new Exception("Invalid last synced id. Must be non-negative.");
        }
    }

    /// <inheritdoc/>
    public async Task SaveLastSyncedExternalAsync(int id)
    {
        if (id >= 0)
        {
            using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            await _lastSyncedRepository.SaveExternalIdAsync(id);
            scope.Complete();
        }
        else
        {
            throw new Exception("Invalid last synced id. Must be non-negative.");
        }

    }

    /// <inheritdoc/>
    public async Task DeleteOlderThanAsync(DateTime date)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        await _lastSyncedRepository.DeleteEntriesOlderThanAsync(date);
        scope.Complete();
    }
}
