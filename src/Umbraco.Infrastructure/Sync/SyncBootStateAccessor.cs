using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Sync
{
    public class SyncBootStateAccessor : ISyncBootStateAccessor
    {
        private readonly ILogger<SyncBootStateAccessor> _logger;
        private readonly LastSyncedFileManager _lastSyncedFileManager;
        private readonly GlobalSettings _globalSettings;
        private readonly ICacheInstructionService _cacheInstructionService;

        private SyncBootState _syncBootState;
        private bool _syncBootStateReady;
        private object _syncBootStateLock;

        public SyncBootStateAccessor(
            ILogger<SyncBootStateAccessor> logger,
            LastSyncedFileManager lastSyncedFileManager,
            IOptions<GlobalSettings> globalSettings,
            ICacheInstructionService cacheInstructionService)
        {
            _logger = logger;
            _lastSyncedFileManager = lastSyncedFileManager;
            _globalSettings = globalSettings.Value;
            _cacheInstructionService = cacheInstructionService;
        }

        public SyncBootState GetSyncBootState()
            => LazyInitializer.EnsureInitialized(
                ref _syncBootState,
                ref _syncBootStateReady,
                ref _syncBootStateLock,
                () =>
                {
                    var lastId = _lastSyncedFileManager.LastSyncedId;
                    if (_cacheInstructionService.IsColdBootRequired(lastId))
                    {
                        return SyncBootState.ColdBoot;
                    }
                    return InitializeColdBootState(lastId);
                });
        
        private SyncBootState InitializeColdBootState(int lastId)
        {
            var coldboot = false;

            // Never synced before.
            if (lastId < 0)
            {
                // We haven't synced - in this case we aren't going to sync the whole thing, we will assume this is a new
                // server and it will need to rebuild it's own caches, e.g. Lucene or the XML cache file.
                _logger.LogWarning("No last synced Id found, this generally means this is a new server/install."
                    + " The server will build its caches and indexes, and then adjust its last synced Id to the latest found in"
                    + " the database and maintain cache updates based on that Id.");

                coldboot = true;
            }
            else
            {
                // Check for how many instructions there are to process, each row contains a count of the number of instructions contained in each
                // row so we will sum these numbers to get the actual count.
                var limit = _globalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount;
                if (_cacheInstructionService.IsInstructionCountOverLimit(lastId, limit, out int count))
                {
                    // Too many instructions, proceed to cold boot.
                    _logger.LogWarning(
                        "The instruction count ({InstructionCount}) exceeds the specified MaxProcessingInstructionCount ({MaxProcessingInstructionCount})."
                        + " The server will skip existing instructions, rebuild its caches and indexes entirely, adjust its last synced Id"
                        + " to the latest found in the database and maintain cache updates based on that Id.",
                        count, limit);

                    coldboot = true;
                }
            }

            return coldboot ? SyncBootState.ColdBoot : SyncBootState.WarmBoot;
        }
    }
}
