using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Sync;

public class SyncBootStateAccessor : ISyncBootStateAccessor
{
    private readonly ICacheInstructionService _cacheInstructionService;
    private readonly ILastSyncedManager _lastSyncedManager;
    private readonly ILogger<SyncBootStateAccessor> _logger;
    private GlobalSettings _globalSettings;

    private SyncBootState _syncBootState;
    private object? _syncBootStateLock;
    private bool _syncBootStateReady;

    public SyncBootStateAccessor(
        ILogger<SyncBootStateAccessor> logger,
        IOptionsMonitor<GlobalSettings> globalSettings,
        ICacheInstructionService cacheInstructionService,
        ILastSyncedManager lastSyncedManager)
    {
        _logger = logger;
        _lastSyncedManager = lastSyncedManager;
        _globalSettings = globalSettings.CurrentValue;
        _cacheInstructionService = cacheInstructionService;

        globalSettings.OnChange(x => _globalSettings = x);
    }

    [Obsolete("Please use the constructor without LastSyncedFileManager. Scheduled for removal in Umbraco 18.")]
    public SyncBootStateAccessor(
        ILogger<SyncBootStateAccessor> logger,
        LastSyncedFileManager lastSyncedFileManager,
        IOptionsMonitor<GlobalSettings> globalSettings,
        ICacheInstructionService cacheInstructionService,
        ILastSyncedManager lastSyncedManager)
        : this(
            logger,
            globalSettings,
            cacheInstructionService,
            lastSyncedManager)
    {
    }

    [Obsolete("Please use the constructor with ILastSyncedManager. Scheduled for removal in Umbraco 18.")]
    public SyncBootStateAccessor(
        ILogger<SyncBootStateAccessor> logger,
        LastSyncedFileManager lastSyncedFileManager,
        IOptionsMonitor<GlobalSettings> globalSettings,
        ICacheInstructionService cacheInstructionService)
        : this(
            logger,
            globalSettings,
            cacheInstructionService,
            StaticServiceProvider.Instance.GetRequiredService<ILastSyncedManager>())
    {
    }

    public SyncBootState GetSyncBootState()
        => LazyInitializer.EnsureInitialized(
            ref _syncBootState,
            ref _syncBootStateReady,
            ref _syncBootStateLock,
            () => InitializeColdBootState(_lastSyncedManager.GetLastSyncedExternalAsync().GetAwaiter().GetResult() ?? -1));

    private SyncBootState InitializeColdBootState(int lastId)
    {
        var coldboot = false;

        // Never synced before.
        if (lastId < 0)
        {
            // We haven't synced - in this case we aren't going to sync the whole thing, we will assume this is a new
            // server and it will need to rebuild it's own caches, e.g. Lucene or the XML cache file.
            _logger.LogWarning("No last synced Id found, this generally means this is a new server/install. "
                               + "A cold boot will be triggered.");

            coldboot = true;
        }
        else
        {
            if (_cacheInstructionService.IsColdBootRequired(lastId))
            {
                _logger.LogWarning(
                    "Last synced Id found {LastSyncedId} but was not found in the database. This generally means this server/install "
                    + " has been idle for too long and the instructions in the database have been pruned. A cold boot will be triggered.",
                    lastId);

                coldboot = true;
            }
            else
            {
                // Check for how many instructions there are to process, each row contains a count of the number of instructions contained in each
                // row so we will sum these numbers to get the actual count.
                var limit = _globalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount;
                if (_cacheInstructionService.IsInstructionCountOverLimit(lastId, limit, out var count))
                {
                    // Too many instructions, proceed to cold boot.
                    _logger.LogWarning("The instruction count ({InstructionCount}) exceeds the specified MaxProcessingInstructionCount ({MaxProcessingInstructionCount}). " + "A cold boot will be triggered.", count, limit);

                    coldboot = true;
                }
            }
        }

        return coldboot ? SyncBootState.ColdBoot : SyncBootState.WarmBoot;
    }
}
