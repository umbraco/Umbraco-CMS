using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;

/// <summary>
/// A background job that prunes cache instructions from the database.
/// </summary>
internal class CacheInstructionsPruningJob : IDistributedBackgroundJob
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ICacheInstructionRepository _cacheInstructionRepository;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly TimeProvider _timeProvider;
    private readonly ILastSyncedManager _lastSyncedManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInstructionsPruningJob"/> class.
    /// </summary>
    /// <param name="globalSettings">The global settings configuration.</param>
    /// <param name="cacheInstructionRepository">The repository for cache instructions.</param>
    /// <param name="scopeProvider">Provides scopes for database operations.</param>
    /// <param name="timeProvider">The time provider.</param>
    /// <param name="lastSyncedManager">The manager for tracking last synced cache instructions.</param>
    public CacheInstructionsPruningJob(
        IOptions<GlobalSettings> globalSettings,
        ICacheInstructionRepository cacheInstructionRepository,
        ICoreScopeProvider scopeProvider,
        TimeProvider timeProvider,
        ILastSyncedManager lastSyncedManager)
    {
        _globalSettings = globalSettings;
        _cacheInstructionRepository = cacheInstructionRepository;
        _scopeProvider = scopeProvider;
        _timeProvider = timeProvider;
        Period = globalSettings.Value.DatabaseServerMessenger.TimeBetweenPruneOperations;
        _lastSyncedManager = lastSyncedManager;
    }

    [Obsolete("Use the constructor with ILastSyncedManager parameter instead. Scheduled for removal in Umbraco 18.")]
    public CacheInstructionsPruningJob(
        IOptions<GlobalSettings> globalSettings,
        ICacheInstructionRepository cacheInstructionRepository,
        ICoreScopeProvider scopeProvider,
        TimeProvider timeProvider)
        : this(
            globalSettings,
            cacheInstructionRepository,
            scopeProvider,
            timeProvider,
            StaticServiceProvider.Instance.GetRequiredService<ILastSyncedManager>())
    {
    }

    public string Name => "CacheInstructionsPruningJob";

    /// <inheritdoc />
    public TimeSpan Period { get; }

    /// <inheritdoc />
    public Task ExecuteAsync()
    {
        DateTimeOffset pruneDate = _timeProvider.GetUtcNow() - _globalSettings.Value.DatabaseServerMessenger.TimeToRetainInstructions;
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _cacheInstructionRepository.DeleteInstructionsOlderThan(pruneDate.DateTime);
            _lastSyncedManager.DeleteOlderThanAsync(pruneDate.DateTime).GetAwaiter().GetResult();
            scope.Complete();
        }

        return Task.CompletedTask;
    }
}
