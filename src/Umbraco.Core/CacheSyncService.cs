using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core;

public class CacheSyncService : ICacheSyncService
{
    private readonly IMachineInfoFactory _machineInfoFactory;
    private readonly CacheRefresherCollection _cacheRefreshers;
    private readonly ICacheInstructionService _cacheInstructionService;

    public CacheSyncService(
        IMachineInfoFactory machineInfoFactory,
        CacheRefresherCollection cacheRefreshers,
        ICacheInstructionService cacheInstructionService)
    {
        _machineInfoFactory = machineInfoFactory;
        _cacheRefreshers = cacheRefreshers;
        _cacheInstructionService = cacheInstructionService;
    }

    /// <inheritdoc />
    public void SyncAll(CancellationToken cancellationToken = default) =>
        _cacheInstructionService.ProcessAllInstructions(
            _cacheRefreshers,
            cancellationToken,
            _machineInfoFactory.GetLocalIdentity());

    /// <inheritdoc />
    public void SyncInternal(CancellationToken cancellationToken) =>
        _cacheInstructionService.ProcessInternalInstructions(
            _cacheRefreshers,
            cancellationToken,
            _machineInfoFactory.GetLocalIdentity());
}
