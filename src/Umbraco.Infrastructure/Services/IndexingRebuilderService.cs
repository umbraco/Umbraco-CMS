using System.Diagnostics.CodeAnalysis;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Services;

/// <inheritdoc />
public class IndexingRebuilderService : IIndexingRebuilderService
{
    private const string OperationTypePrefix = "ExamineIndexRebuild";

    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ILogger<IndexingRebuilderService> _logger;
    private readonly ILongRunningOperationService _longRunningOperationService;
    private readonly IServerRoleAccessor _serverRoleAccessor;

    [ActivatorUtilitiesConstructor]
    public IndexingRebuilderService(
        IIndexRebuilder indexRebuilder,
        ILogger<IndexingRebuilderService> logger,
        ILongRunningOperationService longRunningOperationService,
        IServerRoleAccessor serverRoleAccessor)
    {
        _indexRebuilder = indexRebuilder;
        _logger = logger;
        _longRunningOperationService = longRunningOperationService;
        _serverRoleAccessor = serverRoleAccessor;
    }

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public IndexingRebuilderService(
        AppCaches appCaches,
        IIndexRebuilder indexRebuilder,
        ILogger<IndexingRebuilderService> logger)
        : this(
            indexRebuilder,
            logger,
            StaticServiceProvider.Instance.GetRequiredService<ILongRunningOperationService>(),
            StaticServiceProvider.Instance.GetRequiredService<IServerRoleAccessor>())
    {
    }

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public IndexingRebuilderService(
        IIndexRebuilder indexRebuilder,
        ILogger<IndexingRebuilderService> logger)
        : this(
            indexRebuilder,
            logger,
            StaticServiceProvider.Instance.GetRequiredService<ILongRunningOperationService>(),
            StaticServiceProvider.Instance.GetRequiredService<IServerRoleAccessor>())
    {
    }

    private static string GetOperationType(string indexName) => $"{OperationTypePrefix}:{indexName}";

    // Only use database tracking on servers with write access.
    // Subscriber servers have read-only DB access and don't serve the backoffice.
    private bool UseDatabaseOperationTracking =>
        _serverRoleAccessor.CurrentServerRole is ServerRole.Single or ServerRole.SchedulingPublisher;

    /// <inheritdoc />
    public bool CanRebuild(string indexName) => _indexRebuilder.CanRebuild(indexName);

    /// <inheritdoc />
    [Obsolete("Use TryRebuildAsync instead. Scheduled for removal in Umbraco 19.")]
    public bool TryRebuild(IIndex index, string indexName)
        => TryRebuildAsync(index, indexName).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<bool> TryRebuildAsync(IIndex index, string indexName)
    {
        try
        {
            if (UseDatabaseOperationTracking is false)
            {
                // Subscriber/Unknown servers: delegate directly without operation tracking.
                Attempt<IndexRebuildResult> result = await _indexRebuilder.RebuildIndexAsync(indexName);
                return result.Success;
            }

            Attempt<Guid, LongRunningOperationEnqueueStatus> enqueueResult =
                await _longRunningOperationService.RunAsync(
                    GetOperationType(indexName),
                    async ct =>
                    {
                        // useBackgroundThread: false because ILongRunningOperationService already handles backgrounding.
                        await _indexRebuilder.RebuildIndexAsync(indexName, useBackgroundThread: false);
                    },
                    allowConcurrentExecution: false);

            if (enqueueResult.Status == LongRunningOperationEnqueueStatus.AlreadyRunning)
            {
                _logger.LogWarning("Index rebuild for {IndexName} is already running", indexName);
                return false;
            }

            return enqueueResult.Success;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred rebuilding index {IndexName}", indexName);
            return false;
        }
    }

    /// <inheritdoc />
    [Obsolete("Use IsRebuildingAsync() instead. Scheduled for removal in Umbraco 19.")]
    public bool IsRebuilding(string indexName)
        => IsRebuildingAsync(indexName).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<bool> IsRebuildingAsync(string indexName)
    {
        // Check local in-memory state first (fast path — covers this instance).
        if (await _indexRebuilder.IsRebuildingAsync(indexName))
        {
            return true;
        }

        if (UseDatabaseOperationTracking is false)
        {
            return false;
        }

        // Check database for cross-server visibility (load-balanced backoffice).
        try
        {
            PagedModel<LongRunningOperation> activeOps =
                await _longRunningOperationService.GetByTypeAsync(
                    GetOperationType(indexName),
                    skip: 0,
                    take: 0);

            return activeOps.Total > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check long-running operation status for index rebuild; falling back to local state only");
            return false;
        }
    }
}
