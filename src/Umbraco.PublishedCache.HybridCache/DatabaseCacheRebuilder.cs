using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
/// Rebuilds the published content cache in the database.
/// </summary>
internal sealed class DatabaseCacheRebuilder : IDatabaseCacheRebuilder
{
    private const string NuCacheSerializerKey = "Umbraco.Web.PublishedCache.NuCache.Serializer";
    private const string RebuildOperationName = "DatabaseCacheRebuild";

    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IOptions<NuCacheSettings> _nucacheSettings;
    private readonly IKeyValueService _keyValueService;
    private readonly ILogger<DatabaseCacheRebuilder> _logger;
    private readonly IProfilingLogger _profilingLogger;
    private readonly ILongRunningOperationService _longRunningOperationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseCacheRebuilder"/> class.
    /// </summary>
    public DatabaseCacheRebuilder(
        IDatabaseCacheRepository databaseCacheRepository,
        ICoreScopeProvider coreScopeProvider,
        IOptions<NuCacheSettings> nucacheSettings,
        IKeyValueService keyValueService,
        ILogger<DatabaseCacheRebuilder> logger,
        IProfilingLogger profilingLogger,
        ILongRunningOperationService longRunningOperationService)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _coreScopeProvider = coreScopeProvider;
        _nucacheSettings = nucacheSettings;
        _keyValueService = keyValueService;
        _logger = logger;
        _profilingLogger = profilingLogger;
        _longRunningOperationService = longRunningOperationService;
    }

    /// <inheritdoc/>
    public bool IsRebuilding() => IsRebuildingAsync().GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task<bool> IsRebuildingAsync()
        => (await _longRunningOperationService.GetByTypeAsync(RebuildOperationName, 0, 0)).Total != 0;

    /// <inheritdoc/>
    [Obsolete("Use the overload with the useBackgroundThread parameter. Scheduled for removal in Umbraco 17.")]
    public void Rebuild() => Rebuild(false);

    /// <inheritdoc/>
    [Obsolete("Use RebuildAsync instead. Scheduled for removal in Umbraco 18.")]
    public void Rebuild(bool useBackgroundThread) =>
        RebuildAsync(useBackgroundThread).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task<Attempt<DatabaseCacheRebuildResult>> RebuildAsync(bool useBackgroundThread)
    {
        Attempt<Guid, LongRunningOperationEnqueueStatus> attempt = await _longRunningOperationService.RunAsync(
                RebuildOperationName,
                _ => PerformRebuild(),
                allowConcurrentExecution: false,
                runInBackground: useBackgroundThread);

        if (attempt.Success)
        {
            return Attempt.Succeed(DatabaseCacheRebuildResult.Success);
        }

        return attempt.Status switch
        {
            LongRunningOperationEnqueueStatus.AlreadyRunning => Attempt.Fail(DatabaseCacheRebuildResult.AlreadyRunning),
            _ => throw new InvalidOperationException(
                $"Unexpected status {attempt.Status} when trying to enqueue the database cache rebuild operation."),
        };
    }

    /// <inheritdoc/>
    public void RebuildDatabaseCacheIfSerializerChanged() =>
        RebuildDatabaseCacheIfSerializerChangedAsync().GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task RebuildDatabaseCacheIfSerializerChangedAsync()
    {
        NuCacheSerializerType serializer = _nucacheSettings.Value.NuCacheSerializerType;
        string? currentSerializerValue;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            currentSerializerValue = _keyValueService.GetValue(NuCacheSerializerKey);
        }

        if (Enum.TryParse(currentSerializerValue, out NuCacheSerializerType currentSerializer) && serializer == currentSerializer)
        {
            return;
        }

        _logger.LogWarning(
            "Database cache was serialized using {CurrentSerializer}. Currently configured cache serializer {Serializer}. Rebuilding database cache.",
            currentSerializer,
            serializer);

        using (_profilingLogger.TraceDuration<DatabaseCacheRebuilder>($"Rebuilding database cache with {serializer} serializer"))
        {
            await RebuildAsync(false);
        }
    }

    private Task PerformRebuild()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild([], [], []);

        // If the serializer type has changed, we also need to update it in the key value store.
        var currentSerializerValue = _keyValueService.GetValue(NuCacheSerializerKey);
        if (!Enum.TryParse(currentSerializerValue, out NuCacheSerializerType currentSerializer) ||
            _nucacheSettings.Value.NuCacheSerializerType != currentSerializer)
        {
            _keyValueService.SetValue(NuCacheSerializerKey, _nucacheSettings.Value.NuCacheSerializerType.ToString());
        }

        scope.Complete();
        return Task.CompletedTask;
    }
}
