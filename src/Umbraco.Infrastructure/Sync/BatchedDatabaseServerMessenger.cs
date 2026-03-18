using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Sync;

/// <summary>
///     An <see cref="IServerMessenger" /> implementation that works by storing messages in the database.
/// </summary>
public class BatchedDatabaseServerMessenger : DatabaseServerMessenger
{
    private readonly IRequestCache _requestCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchedDatabaseServerMessenger"/> class.
    /// </summary>
    /// <param name="mainDom">The <see cref="IMainDom"/> instance managing main domain locking.</param>
    /// <param name="cacheRefreshers">A collection of cache refresher implementations used to synchronize cache across servers.</param>
    /// <param name="logger">The logger used for diagnostic and operational logging.</param>
    /// <param name="syncBootStateAccessor">Accessor for the synchronization boot state.</param>
    /// <param name="hostingEnvironment">Provides information about the hosting environment.</param>
    /// <param name="cacheInstructionService">Service for managing cache instructions in the database.</param>
    /// <param name="jsonSerializer">The serializer used for JSON operations.</param>
    /// <param name="requestCache">Request-scoped cache for storing temporary data.</param>
    /// <param name="lastSyncedManager">Manages the state of the last synchronization operation.</param>
    /// <param name="globalSettings">Monitors and provides access to global settings.</param>
    /// <param name="machineInfoFactory">Factory for creating machine information instances.</param>
    public BatchedDatabaseServerMessenger(
        IMainDom mainDom,
        CacheRefresherCollection cacheRefreshers,
        ILogger<BatchedDatabaseServerMessenger> logger,
        ISyncBootStateAccessor syncBootStateAccessor,
        IHostingEnvironment hostingEnvironment,
        ICacheInstructionService cacheInstructionService,
        IJsonSerializer jsonSerializer,
        IRequestCache requestCache,
        ILastSyncedManager lastSyncedManager,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IMachineInfoFactory machineInfoFactory)
        : base(
            mainDom,
            cacheRefreshers,
            logger,
            true,
            syncBootStateAccessor,
            hostingEnvironment,
            cacheInstructionService,
            jsonSerializer,
            globalSettings,
            lastSyncedManager,
            machineInfoFactory)
    {
        _requestCache = requestCache;
    }

    /// <inheritdoc />
    public override void SendMessages()
    {
        ICollection<RefreshInstructionEnvelope>? batch = GetBatch(false);
        if (batch == null)
        {
            return;
        }

        RefreshInstruction[] instructions = batch.SelectMany(x => x.Instructions).ToArray();
        batch.Clear();

        CacheInstructionService.DeliverInstructionsInBatches(instructions, LocalIdentity);
    }

    /// <inheritdoc />
    protected override void DeliverRemote(
        ICacheRefresher refresher,
        MessageType messageType,
        IEnumerable<object>? ids = null,
        string? json = null)
    {
        var idsA = ids?.ToArray();

        if (GetArrayType(idsA, out Type? arrayType) == false)
        {
            throw new ArgumentException("All items must be of the same type, either int or Guid.", nameof(ids));
        }

        BatchMessage(refresher, messageType, idsA, arrayType, json);
    }

    private ICollection<RefreshInstructionEnvelope>? GetBatch(bool create)
    {
        var key = nameof(BatchedDatabaseServerMessenger);

        if (!_requestCache.IsAvailable)
        {
            return null;
        }

        // No thread-safety here because it'll run in only 1 thread (request) at a time.
        var batch = (ICollection<RefreshInstructionEnvelope>?)_requestCache.Get(key);
        if (batch == null && create)
        {
            batch = new List<RefreshInstructionEnvelope>();
            _requestCache.Set(key, batch);
        }

        return batch;
    }

    private void BatchMessage(
        ICacheRefresher refresher,
        MessageType messageType,
        IEnumerable<object>? ids = null,
        Type? idType = null,
        string? json = null)
    {
        ICollection<RefreshInstructionEnvelope>? batch = GetBatch(true);
        IEnumerable<RefreshInstruction> instructions =
            RefreshInstruction.GetInstructions(refresher, JsonSerializer, messageType, ids, idType, json);

        // Batch if we can, else write to DB immediately.
        if (batch == null)
        {
            CacheInstructionService.DeliverInstructionsInBatches(instructions, LocalIdentity);
        }
        else
        {
            batch.Add(new RefreshInstructionEnvelope(refresher, instructions));
        }
    }
}
