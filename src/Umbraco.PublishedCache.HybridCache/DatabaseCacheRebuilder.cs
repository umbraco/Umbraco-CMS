using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
/// Rebuilds the published content cache in the database.
/// </summary>
internal class DatabaseCacheRebuilder : IDatabaseCacheRebuilder
{
    private const string NuCacheSerializerKey = "Umbraco.Web.PublishedCache.NuCache.Serializer";
    private const string IsRebuildingDatabaseCacheRuntimeCacheKey = "temp_database_cache_rebuild_op";

    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IOptions<NuCacheSettings> _nucacheSettings;
    private readonly IKeyValueService _keyValueService;
    private readonly ILogger<DatabaseCacheRebuilder> _logger;
    private readonly IProfilingLogger _profilingLogger;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IAppPolicyCache _runtimeCache;

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
        IBackgroundTaskQueue backgroundTaskQueue,
        IAppPolicyCache runtimeCache)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _coreScopeProvider = coreScopeProvider;
        _nucacheSettings = nucacheSettings;
        _keyValueService = keyValueService;
        _logger = logger;
        _profilingLogger = profilingLogger;
        _backgroundTaskQueue = backgroundTaskQueue;
        _runtimeCache = runtimeCache;
    }

    /// <inheritdoc/>
    public bool IsRebuilding() => _runtimeCache.Get(IsRebuildingDatabaseCacheRuntimeCacheKey) is not null;

    /// <inheritdoc/>
    [Obsolete("Use the overload with the useBackgroundThread parameter. Scheduled for removal in Umbraco 17.")]
    public void Rebuild() => Rebuild(false);

    /// <inheritdoc/>
    public void Rebuild(bool useBackgroundThread)
    {
        if (useBackgroundThread)
        {
            _logger.LogInformation("Starting async background thread for rebuilding database cache.");

            _backgroundTaskQueue.QueueBackgroundWorkItem(
                cancellationToken =>
                {
                    using (ExecutionContext.SuppressFlow())
                    {
                        Task.Run(() => PerformRebuild());
                        return Task.CompletedTask;
                    }
                });
        }
        else
        {
            PerformRebuild();
        }
    }

    private void PerformRebuild()
    {
        try
        {
            SetIsRebuilding();

            using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            _databaseCacheRepository.Rebuild();
            scope.Complete();
        }
        finally
        {
            ClearIsRebuilding();
        }
    }

    private void SetIsRebuilding() => _runtimeCache.Insert(IsRebuildingDatabaseCacheRuntimeCacheKey, () => "tempValue", TimeSpan.FromMinutes(10));

    private void ClearIsRebuilding() => _runtimeCache.Clear(IsRebuildingDatabaseCacheRuntimeCacheKey);

    /// <inheritdoc/>
    public void RebuildDatabaseCacheIfSerializerChanged()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        NuCacheSerializerType serializer = _nucacheSettings.Value.NuCacheSerializerType;
        var currentSerializerValue = _keyValueService.GetValue(NuCacheSerializerKey);

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
            Rebuild(false);
            _keyValueService.SetValue(NuCacheSerializerKey, serializer.ToString());
        }

        scope.Complete();
    }
}
