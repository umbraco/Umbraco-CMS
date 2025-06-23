using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// A background job that prunes cache instructions from the database.
/// </summary>
public class CacheInstructionsPruningJob : IRecurringBackgroundJob
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ICacheInstructionRepository _cacheInstructionRepository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInstructionsPruningJob"/> class.
    /// </summary>
    /// <param name="globalSettings">The global settings configuration.</param>
    /// <param name="cacheInstructionRepository">The repository for cache instructions.</param>
    /// <param name="timeProvider">The time provider.</param>
    public CacheInstructionsPruningJob(
        IOptions<GlobalSettings> globalSettings,
        ICacheInstructionRepository cacheInstructionRepository,
        TimeProvider timeProvider)
    {
        _globalSettings = globalSettings;
        _cacheInstructionRepository = cacheInstructionRepository;
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
        _cacheInstructionRepository.DeleteInstructionsOlderThan(pruneDate.DateTime);
        return Task.CompletedTask;
    }
}
