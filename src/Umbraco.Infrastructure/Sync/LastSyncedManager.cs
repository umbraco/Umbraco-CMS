using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Cms.Infrastructure.Sync;

public class LastSyncedManager : ILastSyncedManager
{
    private readonly ILastSyncedRepository  _lastSyncedRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public LastSyncedManager(ILastSyncedRepository lastSyncedRepository, ICoreScopeProvider coreScopeProvider)
    {
        _lastSyncedRepository = lastSyncedRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    public async Task<int?> GetLastSyncedInternalAsync()
    {
        using ICoreScope coreScope = _coreScopeProvider.CreateCoreScope();
        int? internalId = await _lastSyncedRepository.GetInternal();
        coreScope.Complete();

        return internalId;
    }

    public async Task<int?> GetLastSyncedExternalAsync()
    {
        using ICoreScope coreScope = _coreScopeProvider.CreateCoreScope();
        int? externalId = await _lastSyncedRepository.GetExternal();
        coreScope.Complete();

        return externalId;
    }

    public async void SaveLastSyncedInternalAsync(int id)
    {
        using ICoreScope coreScope = _coreScopeProvider.CreateCoreScope();
        await _lastSyncedRepository.SaveInternal(id);
        coreScope.Complete();
    }

    public async void SaveLastSyncedExternalAsync(int id)
    {
        using ICoreScope coreScope = _coreScopeProvider.CreateCoreScope();
        await _lastSyncedRepository.SaveExternal(id);
        coreScope.Complete();
    }
}
