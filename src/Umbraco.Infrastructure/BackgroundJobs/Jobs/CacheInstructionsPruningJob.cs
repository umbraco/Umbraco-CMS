using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// A background job that prunes cache instructions from the database.
/// </summary>
public class CacheInstructionsPruningJob : IRecurringBackgroundJob
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ICacheInstructionRepository _cacheInstructionRepository;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInstructionsPruningJob"/> class.
    /// </summary>
    /// <param name="scopeProvider">Provides scopes for database operations.</param>
    /// <param name="globalSettings">The global settings configuration.</param>
    /// <param name="cacheInstructionRepository">The repository for cache instructions.</param>
    /// <param name="timeProvider">The time provider.</param>
    public CacheInstructionsPruningJob(
        IOptions<GlobalSettings> globalSettings,
        ICacheInstructionRepository cacheInstructionRepository,
        ICoreScopeProvider scopeProvider,
        TimeProvider timeProvider)
    {
        _globalSettings = globalSettings;
        _cacheInstructionRepository = cacheInstructionRepository;
        _scopeProvider = scopeProvider;
        _timeProvider = timeProvider;
        Period = globalSettings.Value.DatabaseServerMessenger.TimeBetweenPruneOperations;
    }

    /// <inheritdoc />
    public event EventHandler PeriodChanged
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public TimeSpan Period { get; }

    /// <inheritdoc />
    public Task RunJobAsync()
    {
        DateTimeOffset pruneDate = _timeProvider.GetUtcNow() - _globalSettings.Value.DatabaseServerMessenger.TimeToRetainInstructions;
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _cacheInstructionRepository.DeleteInstructionsOlderThan(pruneDate.DateTime);
            scope.Complete();
        }

        return Task.CompletedTask;
    }
}
