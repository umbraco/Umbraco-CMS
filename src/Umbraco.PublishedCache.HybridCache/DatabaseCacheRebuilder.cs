using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
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
    public bool IsRebuilding() => _longRunningOperationService.IsRunning(RebuildOperationName).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public void Rebuild() => Rebuild(false);

    /// <inheritdoc/>
    public void Rebuild(bool useBackgroundThread) =>
        // TODO: Add overload that returns an attempt when the operation is already running
        _longRunningOperationService.Run(
            RebuildOperationName,
            _ =>
            {
                PerformRebuild();
                return Task.CompletedTask;
            },
            allowMultipleRunsOfType: false,
            runInBackground: useBackgroundThread)
            .GetAwaiter().GetResult();

    private void PerformRebuild()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild();
        scope.Complete();
    }

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
