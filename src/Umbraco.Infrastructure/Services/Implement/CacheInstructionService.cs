using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Implement
{
    /// <summary>
    /// Implements <see cref="ICacheInstructionService"/> providing a service for retrieving and saving cache instructions.
    /// </summary>
    public class CacheInstructionService : RepositoryService, ICacheInstructionService
    {
        private readonly IServerRoleAccessor _serverRoleAccessor;
        private readonly CacheRefresherCollection _cacheRefreshers;
        private readonly ICacheInstructionRepository _cacheInstructionRepository;
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<CacheInstructionService> _logger;
        private readonly GlobalSettings _globalSettings;

        private readonly object _locko = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheInstructionService"/> class.
        /// </summary>
        public CacheInstructionService(
            IScopeProvider provider,
            ILoggerFactory loggerFactory,
            IEventMessagesFactory eventMessagesFactory,
            IServerRoleAccessor serverRoleAccessor,
            CacheRefresherCollection cacheRefreshers,
            ICacheInstructionRepository cacheInstructionRepository,
            IProfilingLogger profilingLogger,
            ILogger<CacheInstructionService> logger,
            IOptions<GlobalSettings> globalSettings)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _serverRoleAccessor = serverRoleAccessor;
            _cacheRefreshers = cacheRefreshers;
            _cacheInstructionRepository = cacheInstructionRepository;
            _profilingLogger = profilingLogger;
            _logger = logger;
            _globalSettings = globalSettings.Value;
        }

        /// <inheritdoc/>
        public CacheInstructionServiceInitializationResult EnsureInitialized(bool released, int lastId)
        {
            using (IScope scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                lastId = EnsureInstructions(lastId); // reset _lastId if instructions are missing
                return Initialize(released, lastId); // boot
            }
        }

        /// <summary>
        /// Ensure that the last instruction that was processed is still in the database.
        /// </summary>
        /// <remarks>
        /// If the last instruction is not in the database anymore, then the messenger
        /// should not try to process any instructions, because some instructions might be lost,
        /// and it should instead cold-boot.
        /// However, if the last synced instruction id is '0' and there are '0' records, then this indicates
        /// that it's a fresh site and no user actions have taken place, in this circumstance we do not want to cold
        /// boot. See: http://issues.umbraco.org/issue/U4-8627
        /// </remarks>
        private int EnsureInstructions(int lastId)
        {
            if (lastId == 0)
            {
                var count = _cacheInstructionRepository.CountAll();

                // If there are instructions but we haven't synced, then a cold boot is necessary.
                if (count > 0)
                {
                    lastId = -1;
                }
            }
            else
            {
                // If the last synced instruction is not found in the db, then a cold boot is necessary.
                if (!_cacheInstructionRepository.Exists(lastId))
                {
                    lastId = -1;
                }
            }

            return lastId;
        }

        /// <summary>
        /// Initializes a server that has never synchronized before.
        /// </summary>
        /// <remarks>
        /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// Callers MUST ensure thread-safety.
        /// </remarks>
        private CacheInstructionServiceInitializationResult Initialize(bool released, int lastId)
        {
            lock (_locko)
            {
                if (released)
                {
                    return CacheInstructionServiceInitializationResult.AsUninitialized();
                }

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
                    // Check for how many instructions there are to process. Each row contains a count of the number of instructions contained in each
                    // row so we will sum these numbers to get the actual count.
                    var count = _cacheInstructionRepository.CountPendingInstructions(lastId);
                    if (count > _globalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount)
                    {
                        // Too many instructions, proceed to cold boot
                        _logger.LogWarning(
                            "The instruction count ({InstructionCount}) exceeds the specified MaxProcessingInstructionCount ({MaxProcessingInstructionCount})."
                            + " The server will skip existing instructions, rebuild its caches and indexes entirely, adjust its last synced Id"
                            + " to the latest found in the database and maintain cache updates based on that Id.",
                            count, _globalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount);

                        coldboot = true;
                    }
                }

                // If cold boot is required, go get the last id in the db and store it.
                // Note: do it BEFORE initializing otherwise some instructions might get lost.
                // When doing it before, some instructions might run twice - not an issue.
                var maxId = coldboot
                    ? _cacheInstructionRepository.GetMaxId()
                    : 0;

                return CacheInstructionServiceInitializationResult.AsInitialized(coldboot, maxId, lastId);
            }
        }

        /// <inheritdoc/>
        public void DeliverInstructions(IEnumerable<RefreshInstruction> instructions, string localIdentity)
        {
            CacheInstruction entity = CreateCacheInstruction(instructions, localIdentity);

            using (IScope scope = ScopeProvider.CreateScope())
            {
                _cacheInstructionRepository.Add(entity);
                scope.Complete();
            }
        }

        /// <inheritdoc/>
        public void DeliverInstructionsInBatches(IEnumerable<RefreshInstruction> instructions, string localIdentity)
        {
            // Write the instructions but only create JSON blobs with a max instruction count equal to MaxProcessingInstructionCount.
            using (IScope scope = ScopeProvider.CreateScope())
            {
                foreach (IEnumerable<RefreshInstruction> instructionsBatch in instructions.InGroupsOf(_globalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount))
                {
                    CacheInstruction entity = CreateCacheInstruction(instructionsBatch, localIdentity);
                    _cacheInstructionRepository.Add(entity);
                }

                scope.Complete();
            }
        }

        private CacheInstruction CreateCacheInstruction(IEnumerable<RefreshInstruction> instructions, string localIdentity) =>
            new CacheInstruction(0, DateTime.UtcNow, JsonConvert.SerializeObject(instructions, Formatting.None), localIdentity, instructions.Sum(x => x.JsonIdCount));

        /// <inheritdoc/>
        public CacheInstructionServiceProcessInstructionsResult ProcessInstructions(bool released, string localIdentity, DateTime lastPruned)
        {
            using (_profilingLogger.DebugDuration<CacheInstructionService>("Syncing from database..."))
            using (IScope scope = ScopeProvider.CreateScope())
            {
                var numberOfInstructionsProcessed = ProcessDatabaseInstructions(released, localIdentity, out int lastId);

                // Check for pruning throttling.
                if (released || (DateTime.UtcNow - lastPruned) <= _globalSettings.DatabaseServerMessenger.TimeBetweenPruneOperations)
                {
                    scope.Complete();
                    return CacheInstructionServiceProcessInstructionsResult.AsCompleted(numberOfInstructionsProcessed, lastId);
                }

                var instructionsWerePruned = false;
                switch (_serverRoleAccessor.CurrentServerRole)
                {
                    case ServerRole.Single:
                    case ServerRole.Master:
                        PruneOldInstructions();
                        instructionsWerePruned = true;
                        break;
                }

                scope.Complete();

                return instructionsWerePruned
                    ? CacheInstructionServiceProcessInstructionsResult.AsCompletedAndPruned(numberOfInstructionsProcessed, lastId)
                    : CacheInstructionServiceProcessInstructionsResult.AsCompleted(numberOfInstructionsProcessed, lastId);
            }
        }

        /// <summary>
        /// Process instructions from the database.
        /// </summary>
        /// <remarks>
        /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// </remarks>
        /// <returns>Number of instructions processed.</returns>
        private int ProcessDatabaseInstructions(bool released, string localIdentity, out int lastId)
        {
            // NOTE:
            // We 'could' recurse to ensure that no remaining instructions are pending in the table before proceeding but I don't think that
            // would be a good idea since instructions could keep getting added and then all other threads will probably get stuck from serving requests
            // (depending on what the cache refreshers are doing). I think it's best we do the one time check, process them and continue, if there are
            // pending requests after being processed, they'll just be processed on the next poll.
            //
            // TODO: not true if we're running on a background thread, assuming we can?

            // Only retrieve the top 100 (just in case there are tons).
            // Even though MaxProcessingInstructionCount is by default 1000 we still don't want to process that many
            // rows in one request thread since each row can contain a ton of instructions (until 7.5.5 in which case
            // a row can only contain MaxProcessingInstructionCount).
            const int MaxInstructionsToRetrieve = 100;

            // Only process instructions coming from a remote server, and ignore instructions coming from
            // the local server as they've already been processed. We should NOT assume that the sequence of
            // instructions in the database makes any sense whatsoever, because it's all async.

            // Tracks which ones have already been processed to avoid duplicates
            var processed = new HashSet<RefreshInstruction>();
            var numberOfInstructionsProcessed = 0;

            // It would have been nice to do this in a Query instead of Fetch using a data reader to save
            // some memory however we cannot do that because inside of this loop the cache refreshers are also
            // performing some lookups which cannot be done with an active reader open.
            lastId = 0;
            foreach (CacheInstruction instruction in _cacheInstructionRepository.GetPendingInstructions(lastId, MaxInstructionsToRetrieve))
            {
                // If this flag gets set it means we're shutting down! In this case, we need to exit asap and cannot
                // continue processing anything otherwise we'll hold up the app domain shutdown.
                if (released)
                {
                    break;
                }

                if (instruction.OriginIdentity == localIdentity)
                {
                    // Just skip that local one but update lastId nevertheless.
                    lastId = instruction.Id;
                    continue;
                }

                // Deserialize remote instructions & skip if it fails.
                if (!TryDeserializeInstructions(instruction, out JArray jsonInstructions))
                {
                    lastId = instruction.Id; // skip
                    continue;
                }

                List<RefreshInstruction> instructionBatch = GetAllInstructions(jsonInstructions);

                // Process as per-normal.
                var success = ProcessDatabaseInstructions(instructionBatch, instruction, processed, released, ref lastId);

                // If they couldn't be all processed (i.e. we're shutting down) then exit.
                if (success == false)
                {
                    _logger.LogInformation("The current batch of instructions was not processed, app is shutting down");
                    break;
                }

                numberOfInstructionsProcessed++;
            }

            return numberOfInstructionsProcessed;
        }

        /// <summary>
        /// Attempts to deserialize the instructions to a JArray.
        /// </summary>
        private bool TryDeserializeInstructions(CacheInstruction instruction, out JArray jsonInstructions)
        {
            try
            {
                jsonInstructions = JsonConvert.DeserializeObject<JArray>(instruction.Instructions);
                return true;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize instructions ({DtoId}: '{DtoInstructions}').",
                    instruction.Id,
                    instruction.Instructions);
                jsonInstructions = null;
                return false;
            }
        }

        /// <summary>
        /// Parses out the individual instructions to be processed.
        /// </summary>
        private static List<RefreshInstruction> GetAllInstructions(IEnumerable<JToken> jsonInstructions)
        {
            var result = new List<RefreshInstruction>();
            foreach (JToken jsonItem in jsonInstructions)
            {
                // Could be a JObject in which case we can convert to a RefreshInstruction.
                // Otherwise it could be another JArray - in which case we'll iterate that.
                if (jsonItem is JObject jsonObj)
                {
                    RefreshInstruction instruction = jsonObj.ToObject<RefreshInstruction>();
                    result.Add(instruction);
                }
                else
                {
                    var jsonInnerArray = (JArray)jsonItem;
                    result.AddRange(GetAllInstructions(jsonInnerArray)); // recurse
                }
            }

            return result;
        }

        /// <summary>
        /// Processes the instruction batch and checks for errors.
        /// </summary>
        /// <param name="processed">
        /// Tracks which instructions have already been processed to avoid duplicates
        /// </param>
        /// Returns true if all instructions in the batch were processed, otherwise false if they could not be due to the app being shut down
        /// </returns>
        private bool ProcessDatabaseInstructions(IReadOnlyCollection<RefreshInstruction> instructionBatch, CacheInstruction instruction, HashSet<RefreshInstruction> processed, bool released, ref int lastId)
        {
            // Execute remote instructions & update lastId.
            try
            {
                var result = NotifyRefreshers(instructionBatch, processed, released);
                if (result)
                {
                    // If all instructions were processed, set the last id.
                    lastId = instruction.Id;
                }

                return result;
            }
            //catch (ThreadAbortException ex)
            //{
            //    //This will occur if the instructions processing is taking too long since this is occurring on a request thread.
            //    // Or possibly if IIS terminates the appdomain. In any case, we should deal with this differently perhaps...
            //}
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "DISTRIBUTED CACHE IS NOT UPDATED. Failed to execute instructions ({DtoId}: '{DtoInstructions}'). Instruction is being skipped/ignored",
                    instruction.Id,
                    instruction.Instructions);

                // We cannot throw here because this invalid instruction will just keep getting processed over and over and errors
                // will be thrown over and over. The only thing we can do is ignore and move on.
                lastId = instruction.Id;
                return false;
            }
        }

        /// <summary>
        /// Executes the instructions against the cache refresher instances.
        /// </summary>
        /// <returns>
        /// Returns true if all instructions were processed, otherwise false if the processing was interrupted (i.e. by app shutdown).
        /// </returns>
        private bool NotifyRefreshers(IEnumerable<RefreshInstruction> instructions, HashSet<RefreshInstruction> processed, bool released)
        {
            foreach (RefreshInstruction instruction in instructions)
            {
                // Check if the app is shutting down, we need to exit if this happens.
                if (released)
                {
                    return false;
                }

                // This has already been processed.
                if (processed.Contains(instruction))
                {
                    continue;
                }

                switch (instruction.RefreshType)
                {
                    case RefreshMethodType.RefreshAll:
                        RefreshAll(instruction.RefresherId);
                        break;
                    case RefreshMethodType.RefreshByGuid:
                        RefreshByGuid(instruction.RefresherId, instruction.GuidId);
                        break;
                    case RefreshMethodType.RefreshById:
                        RefreshById(instruction.RefresherId, instruction.IntId);
                        break;
                    case RefreshMethodType.RefreshByIds:
                        RefreshByIds(instruction.RefresherId, instruction.JsonIds);
                        break;
                    case RefreshMethodType.RefreshByJson:
                        RefreshByJson(instruction.RefresherId, instruction.JsonPayload);
                        break;
                    case RefreshMethodType.RemoveById:
                        RemoveById(instruction.RefresherId, instruction.IntId);
                        break;
                }

                processed.Add(instruction);
            }

            return true;
        }

        private void RefreshAll(Guid uniqueIdentifier)
        {
            ICacheRefresher refresher = GetRefresher(uniqueIdentifier);
            refresher.RefreshAll();
        }

        private void RefreshByGuid(Guid uniqueIdentifier, Guid id)
        {
            ICacheRefresher refresher = GetRefresher(uniqueIdentifier);
            refresher.Refresh(id);
        }

        private void RefreshById(Guid uniqueIdentifier, int id)
        {
            ICacheRefresher refresher = GetRefresher(uniqueIdentifier);
            refresher.Refresh(id);
        }

        private void RefreshByIds(Guid uniqueIdentifier, string jsonIds)
        {
            ICacheRefresher refresher = GetRefresher(uniqueIdentifier);
            foreach (var id in JsonConvert.DeserializeObject<int[]>(jsonIds))
            {
                refresher.Refresh(id);
            }
        }

        private void RefreshByJson(Guid uniqueIdentifier, string jsonPayload)
        {
            IJsonCacheRefresher refresher = GetJsonRefresher(uniqueIdentifier);
            refresher.Refresh(jsonPayload);
        }

        private void RemoveById(Guid uniqueIdentifier, int id)
        {
            ICacheRefresher refresher = GetRefresher(uniqueIdentifier);
            refresher.Remove(id);
        }

        private ICacheRefresher GetRefresher(Guid id)
        {
            ICacheRefresher refresher = _cacheRefreshers[id];
            if (refresher == null)
            {
                throw new InvalidOperationException("Cache refresher with ID \"" + id + "\" does not exist.");
            }

            return refresher;
        }

        private IJsonCacheRefresher GetJsonRefresher(Guid id) => GetJsonRefresher(GetRefresher(id));

        private static IJsonCacheRefresher GetJsonRefresher(ICacheRefresher refresher)
        {
            if (refresher is not IJsonCacheRefresher jsonRefresher)
            {
                throw new InvalidOperationException("Cache refresher with ID \"" + refresher.RefresherUniqueId + "\" does not implement " + typeof(IJsonCacheRefresher) + ".");
            }

            return jsonRefresher;
        }

        /// <summary>
        /// Remove old instructions from the database
        /// </summary>
        /// <remarks>
        /// Always leave the last (most recent) record in the db table, this is so that not all instructions are removed which would cause
        /// the site to cold boot if there's been no instruction activity for more than TimeToRetainInstructions.
        /// See: http://issues.umbraco.org/issue/U4-7643#comment=67-25085
        /// </remarks>
        private void PruneOldInstructions()
        {
            DateTime pruneDate = DateTime.UtcNow - _globalSettings.DatabaseServerMessenger.TimeToRetainInstructions;
            _cacheInstructionRepository.DeleteInstructionsOlderThan(pruneDate);
        }
    }
}
