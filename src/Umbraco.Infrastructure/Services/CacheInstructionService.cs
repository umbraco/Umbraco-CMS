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
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms
{
    namespace Core.Services.Implement
    {
        [Obsolete("Scheduled for removal in v12")]
        public class CacheInstructionService : Infrastructure.Services.CacheInstructionService
        {
            public CacheInstructionService(
                ICoreScopeProvider provider,
                ILoggerFactory loggerFactory,
                IEventMessagesFactory eventMessagesFactory,
                ICacheInstructionRepository cacheInstructionRepository,
                IProfilingLogger profilingLogger,
                ILogger<Infrastructure.Services.CacheInstructionService> logger,
                IOptions<GlobalSettings> globalSettings)
                : base(
                    provider,
                    loggerFactory,
                    eventMessagesFactory,
                    cacheInstructionRepository,
                    profilingLogger,
                    logger,
                    globalSettings)
            {
            }
        }
    }

    namespace Infrastructure.Services
    {
        /// <summary>
        ///     Implements <see cref="ICacheInstructionService" /> providing a service for retrieving and saving cache
        ///     instructions.
        /// </summary>
        public class CacheInstructionService : RepositoryService, ICacheInstructionService
        {
            private readonly ICacheInstructionRepository _cacheInstructionRepository;
            private readonly GlobalSettings _globalSettings;
            private readonly ILogger<CacheInstructionService> _logger;
            private readonly IProfilingLogger _profilingLogger;

            /// <summary>
            ///     Initializes a new instance of the <see cref="CacheInstructionService" /> class.
            /// </summary>
            public CacheInstructionService(
                ICoreScopeProvider provider,
                ILoggerFactory loggerFactory,
                IEventMessagesFactory eventMessagesFactory,
                ICacheInstructionRepository cacheInstructionRepository,
                IProfilingLogger profilingLogger,
                ILogger<CacheInstructionService> logger,
                IOptions<GlobalSettings> globalSettings)
                : base(provider, loggerFactory, eventMessagesFactory)
            {
                _cacheInstructionRepository = cacheInstructionRepository;
                _profilingLogger = profilingLogger;
                _logger = logger;
                _globalSettings = globalSettings.Value;
            }

            /// <inheritdoc />
            public bool IsColdBootRequired(int lastId)
            {
                using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
                if (lastId <= 0)
                {
                    var count = _cacheInstructionRepository.CountAll();

                    // If there are instructions but we haven't synced, then a cold boot is necessary.
                    if (count > 0)
                    {
                        return true;
                    }
                }
                else
                {
                    // If the last synced instruction is not found in the db, then a cold boot is necessary.
                    if (!_cacheInstructionRepository.Exists(lastId))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <inheritdoc />
            public bool IsInstructionCountOverLimit(int lastId, int limit, out int count)
            {
                using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
                // Check for how many instructions there are to process, each row contains a count of the number of instructions contained in each
                // row so we will sum these numbers to get the actual count.
                count = _cacheInstructionRepository.CountPendingInstructions(lastId);
                return count > limit;
            }

            /// <inheritdoc />
            public int GetMaxInstructionId()
            {
                using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
                return _cacheInstructionRepository.GetMaxId();
            }

            /// <inheritdoc />
            public void DeliverInstructions(IEnumerable<RefreshInstruction> instructions, string localIdentity)
            {
                CacheInstruction entity = CreateCacheInstruction(instructions, localIdentity);

                using (ICoreScope scope = ScopeProvider.CreateCoreScope())
                {
                    _cacheInstructionRepository.Add(entity);
                    scope.Complete();
                }
            }

            /// <inheritdoc />
            public void DeliverInstructionsInBatches(IEnumerable<RefreshInstruction> instructions, string localIdentity)
            {
                // Write the instructions but only create JSON blobs with a max instruction count equal to MaxProcessingInstructionCount.
                using (ICoreScope scope = ScopeProvider.CreateCoreScope())
                {
                    foreach (IEnumerable<RefreshInstruction> instructionsBatch in instructions.InGroupsOf(
                                 _globalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount))
                    {
                        CacheInstruction entity = CreateCacheInstruction(instructionsBatch, localIdentity);
                        _cacheInstructionRepository.Add(entity);
                    }

                    scope.Complete();
                }
            }

            /// <inheritdoc />
            public ProcessInstructionsResult ProcessInstructions(
                CacheRefresherCollection cacheRefreshers,
                ServerRole serverRole,
                CancellationToken cancellationToken,
                string localIdentity,
                DateTime lastPruned,
                int lastId)
            {
                using (_profilingLogger.DebugDuration<CacheInstructionService>("Syncing from database..."))
                using (ICoreScope scope = ScopeProvider.CreateCoreScope())
                {
                    var numberOfInstructionsProcessed = ProcessDatabaseInstructions(cacheRefreshers, cancellationToken, localIdentity, ref lastId);

                    // Check for pruning throttling.
                    if (cancellationToken.IsCancellationRequested || DateTime.UtcNow - lastPruned <=
                        _globalSettings.DatabaseServerMessenger.TimeBetweenPruneOperations)
                    {
                        scope.Complete();
                        return ProcessInstructionsResult.AsCompleted(numberOfInstructionsProcessed, lastId);
                    }

                    var instructionsWerePruned = false;
                    switch (serverRole)
                    {
                        case ServerRole.Single:
                        case ServerRole.SchedulingPublisher:
                            PruneOldInstructions();
                            instructionsWerePruned = true;
                            break;
                    }

                    scope.Complete();

                    return instructionsWerePruned
                        ? ProcessInstructionsResult.AsCompletedAndPruned(numberOfInstructionsProcessed, lastId)
                        : ProcessInstructionsResult.AsCompleted(numberOfInstructionsProcessed, lastId);
                }
            }

            private CacheInstruction CreateCacheInstruction(IEnumerable<RefreshInstruction> instructions, string localIdentity) =>
                new(0, DateTime.UtcNow, JsonConvert.SerializeObject(instructions, Formatting.None), localIdentity, instructions.Sum(x => x.JsonIdCount));

            /// <summary>
            ///     Process instructions from the database.
            /// </summary>
            /// <remarks>
            ///     Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
            /// </remarks>
            /// <returns>Number of instructions processed.</returns>
            private int ProcessDatabaseInstructions(CacheRefresherCollection cacheRefreshers, CancellationToken cancellationToken, string localIdentity, ref int lastId)
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
                IEnumerable<CacheInstruction> pendingInstructions =
                    _cacheInstructionRepository.GetPendingInstructions(lastId, MaxInstructionsToRetrieve);
                lastId = 0;
                foreach (CacheInstruction instruction in pendingInstructions)
                {
                    // If this flag gets set it means we're shutting down! In this case, we need to exit asap and cannot
                    // continue processing anything otherwise we'll hold up the app domain shutdown.
                    if (cancellationToken.IsCancellationRequested)
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
                    if (!TryDeserializeInstructions(instruction, out JArray? jsonInstructions))
                    {
                        lastId = instruction.Id; // skip
                        continue;
                    }

                    List<RefreshInstruction> instructionBatch = GetAllInstructions(jsonInstructions);

                    // Process as per-normal.
                    var success = ProcessDatabaseInstructions(cacheRefreshers, instructionBatch, instruction, processed, cancellationToken, ref lastId);

                    // If they couldn't be all processed (i.e. we're shutting down) then exit.
                    if (success == false)
                    {
                        _logger.LogInformation(
                            "The current batch of instructions was not processed, app is shutting down");
                        break;
                    }

                    numberOfInstructionsProcessed++;
                }

                return numberOfInstructionsProcessed;
            }

            /// <summary>
            ///     Attempts to deserialize the instructions to a JArray.
            /// </summary>
            private bool TryDeserializeInstructions(CacheInstruction instruction, out JArray? jsonInstructions)
            {
                if (instruction.Instructions is null)
                {
                    _logger.LogError("Failed to deserialize instructions ({DtoId}: 'null').", instruction.Id);
                    jsonInstructions = null;
                    return false;
                }

                try
                {
                    jsonInstructions = JsonConvert.DeserializeObject<JArray>(instruction.Instructions);
                    return true;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize instructions ({DtoId}: '{DtoInstructions}').", instruction.Id, instruction.Instructions);
                    jsonInstructions = null;
                    return false;
                }
            }

            /// <summary>
            ///     Parses out the individual instructions to be processed.
            /// </summary>
            private static List<RefreshInstruction> GetAllInstructions(IEnumerable<JToken>? jsonInstructions)
            {
                var result = new List<RefreshInstruction>();
                if (jsonInstructions is null)
                {
                    return result;
                }

                foreach (JToken jsonItem in jsonInstructions)
                {
                    // Could be a JObject in which case we can convert to a RefreshInstruction.
                    // Otherwise it could be another JArray - in which case we'll iterate that.
                    if (jsonItem is JObject jsonObj)
                    {
                        RefreshInstruction? instruction = jsonObj.ToObject<RefreshInstruction>();
                        if (instruction is not null)
                        {
                            result.Add(instruction);
                        }
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
            ///     Processes the instruction batch and checks for errors.
            /// </summary>
            /// <param name="processed">
            ///     Tracks which instructions have already been processed to avoid duplicates
            /// </param>
            /// <returns>
            /// Returns true if all instructions in the batch were processed, otherwise false if they could not be due to the app being shut down
            /// </returns>
            private bool ProcessDatabaseInstructions(
                CacheRefresherCollection cacheRefreshers,
                IReadOnlyCollection<RefreshInstruction> instructionBatch,
                CacheInstruction instruction,
                HashSet<RefreshInstruction> processed,
                CancellationToken cancellationToken,
                ref int lastId)
            {
                // Execute remote instructions & update lastId.
                try
                {
                    var result = NotifyRefreshers(cacheRefreshers, instructionBatch, processed, cancellationToken);
                    if (result)
                    {
                        // If all instructions were processed, set the last id.
                        lastId = instruction.Id;
                    }

                    return result;
                }
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
            ///     Executes the instructions against the cache refresher instances.
            /// </summary>
            /// <returns>
            ///     Returns true if all instructions were processed, otherwise false if the processing was interrupted (i.e. by app
            ///     shutdown).
            /// </returns>
            private bool NotifyRefreshers(
                CacheRefresherCollection cacheRefreshers,
                IEnumerable<RefreshInstruction> instructions,
                HashSet<RefreshInstruction> processed,
                CancellationToken cancellationToken)
            {
                foreach (RefreshInstruction instruction in instructions)
                {
                    // Check if the app is shutting down, we need to exit if this happens.
                    if (cancellationToken.IsCancellationRequested)
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
                            RefreshAll(cacheRefreshers, instruction.RefresherId);
                            break;
                        case RefreshMethodType.RefreshByGuid:
                            RefreshByGuid(cacheRefreshers, instruction.RefresherId, instruction.GuidId);
                            break;
                        case RefreshMethodType.RefreshById:
                            RefreshById(cacheRefreshers, instruction.RefresherId, instruction.IntId);
                            break;
                        case RefreshMethodType.RefreshByIds:
                            RefreshByIds(cacheRefreshers, instruction.RefresherId, instruction.JsonIds);
                            break;
                        case RefreshMethodType.RefreshByJson:
                            RefreshByJson(cacheRefreshers, instruction.RefresherId, instruction.JsonPayload);
                            break;
                        case RefreshMethodType.RemoveById:
                            RemoveById(cacheRefreshers, instruction.RefresherId, instruction.IntId);
                            break;
                    }

                    processed.Add(instruction);
                }

                return true;
            }

            private void RefreshAll(CacheRefresherCollection cacheRefreshers, Guid uniqueIdentifier)
            {
                ICacheRefresher refresher = GetRefresher(cacheRefreshers, uniqueIdentifier);
                refresher.RefreshAll();
            }

            private void RefreshByGuid(CacheRefresherCollection cacheRefreshers, Guid uniqueIdentifier, Guid id)
            {
                ICacheRefresher refresher = GetRefresher(cacheRefreshers, uniqueIdentifier);
                refresher.Refresh(id);
            }

            private void RefreshById(CacheRefresherCollection cacheRefreshers, Guid uniqueIdentifier, int id)
            {
                ICacheRefresher refresher = GetRefresher(cacheRefreshers, uniqueIdentifier);
                refresher.Refresh(id);
            }

            private void RefreshByIds(CacheRefresherCollection cacheRefreshers, Guid uniqueIdentifier, string? jsonIds)
            {
                ICacheRefresher refresher = GetRefresher(cacheRefreshers, uniqueIdentifier);
                if (jsonIds is null)
                {
                    return;
                }

                var ids = JsonConvert.DeserializeObject<int[]>(jsonIds);
                if (ids is not null)
                {
                    foreach (var id in ids)
                    {
                        refresher.Refresh(id);
                    }
                }
            }

            private void RefreshByJson(CacheRefresherCollection cacheRefreshers, Guid uniqueIdentifier, string? jsonPayload)
            {
                IJsonCacheRefresher refresher = GetJsonRefresher(cacheRefreshers, uniqueIdentifier);
                if (jsonPayload is not null)
                {
                    refresher.Refresh(jsonPayload);
                }
            }

            private void RemoveById(CacheRefresherCollection cacheRefreshers, Guid uniqueIdentifier, int id)
            {
                ICacheRefresher refresher = GetRefresher(cacheRefreshers, uniqueIdentifier);
                refresher.Remove(id);
            }

            private ICacheRefresher GetRefresher(CacheRefresherCollection cacheRefreshers, Guid id)
            {
                ICacheRefresher? refresher = cacheRefreshers[id];
                if (refresher == null)
                {
                    throw new InvalidOperationException("Cache refresher with ID \"" + id + "\" does not exist.");
                }

                return refresher;
            }

            private IJsonCacheRefresher GetJsonRefresher(CacheRefresherCollection cacheRefreshers, Guid id) =>
                GetJsonRefresher(GetRefresher(cacheRefreshers, id));

            private static IJsonCacheRefresher GetJsonRefresher(ICacheRefresher refresher)
            {
                if (refresher is not IJsonCacheRefresher jsonRefresher)
                {
                    throw new InvalidOperationException("Cache refresher with ID \"" + refresher.RefresherUniqueId +
                                                        "\" does not implement " + typeof(IJsonCacheRefresher) + ".");
                }

                return jsonRefresher;
            }

            /// <summary>
            ///     Remove old instructions from the database
            /// </summary>
            /// <remarks>
            ///     Always leave the last (most recent) record in the db table, this is so that not all instructions are removed which
            ///     would cause
            ///     the site to cold boot if there's been no instruction activity for more than TimeToRetainInstructions.
            ///     See: http://issues.umbraco.org/issue/U4-7643#comment=67-25085
            /// </remarks>
            private void PruneOldInstructions()
            {
                DateTime pruneDate = DateTime.UtcNow - _globalSettings.DatabaseServerMessenger.TimeToRetainInstructions;
                _cacheInstructionRepository.DeleteInstructionsOlderThan(pruneDate);
            }
        }
    }
}
