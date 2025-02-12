using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class DatabaseCacheRebuilder : IDatabaseCacheRebuilder
{
    private const string NuCacheSerializerKey = "Umbraco.Web.PublishedCache.NuCache.Serializer";
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IOptions<NuCacheSettings> _nucacheSettings;
    private readonly IKeyValueService _keyValueService;
    private readonly ILogger<DatabaseCacheRebuilder> _logger;
    private readonly IProfilingLogger _profilingLogger;

    public DatabaseCacheRebuilder(
        IDatabaseCacheRepository databaseCacheRepository,
        ICoreScopeProvider coreScopeProvider,
        IOptions<NuCacheSettings> nucacheSettings,
        IKeyValueService keyValueService,
        ILogger<DatabaseCacheRebuilder> logger, IProfilingLogger profilingLogger)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _coreScopeProvider = coreScopeProvider;
        _nucacheSettings = nucacheSettings;
        _keyValueService = keyValueService;
        _logger = logger;
        _profilingLogger = profilingLogger;
    }

    public void Rebuild()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild();
        scope.Complete();
    }

    public void RebuildDatabaseCacheIfSerializerChanged()
    {
        using var scope = _coreScopeProvider.CreateCoreScope();
        NuCacheSerializerType serializer = _nucacheSettings.Value.NuCacheSerializerType;
        var currentSerializerValue = _keyValueService.GetValue(NuCacheSerializerKey);

        if (Enum.TryParse(currentSerializerValue, out NuCacheSerializerType currentSerializer) && serializer == currentSerializer)
        {
            return;
        }

        _logger.LogWarning(
            "Database cache was serialized using {CurrentSerializer}. Currently configured cache serializer {Serializer}. Rebuilding database cache.",
            currentSerializer, serializer);

        using (_profilingLogger.TraceDuration<DatabaseCacheRebuilder>($"Rebuilding database cache with {serializer} serializer"))
        {
            Rebuild();
            _keyValueService.SetValue(NuCacheSerializerKey, serializer.ToString());
        }

        scope.Complete();
    }
}
