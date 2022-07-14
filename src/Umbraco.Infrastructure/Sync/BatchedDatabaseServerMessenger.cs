using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Sync;

/// <summary>
///     An <see cref="IServerMessenger" /> implementation that works by storing messages in the database.
/// </summary>
public class BatchedDatabaseServerMessenger : DatabaseServerMessenger
{
    private readonly IRequestAccessor _requestAccessor;
    private readonly IRequestCache _requestCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BatchedDatabaseServerMessenger" /> class.
    /// </summary>
    public BatchedDatabaseServerMessenger(
        IMainDom mainDom,
        CacheRefresherCollection cacheRefreshers,
        IServerRoleAccessor serverRoleAccessor,
        ILogger<BatchedDatabaseServerMessenger> logger,
        ISyncBootStateAccessor syncBootStateAccessor,
        IHostingEnvironment hostingEnvironment,
        ICacheInstructionService cacheInstructionService,
        IJsonSerializer jsonSerializer,
        IRequestCache requestCache,
        IRequestAccessor requestAccessor,
        LastSyncedFileManager lastSyncedFileManager,
        IOptionsMonitor<GlobalSettings> globalSettings)
        : base(mainDom, cacheRefreshers, serverRoleAccessor, logger, true, syncBootStateAccessor, hostingEnvironment,
            cacheInstructionService, jsonSerializer, lastSyncedFileManager, globalSettings)
    {
        _requestCache = requestCache;
        _requestAccessor = requestAccessor;
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
    protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType,
        IEnumerable<object>? ids = null, string? json = null)
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
