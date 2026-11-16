using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Search.Provider.Examine.Services;
using IndexOptions = Umbraco.Cms.Core.Search.Configuration.IndexOptions;

namespace Umbraco.Cms.Search.Provider.Examine.BackgroundJobs;

/// <summary>
/// On application startup, rebuilds any registered index whose active physical Lucene index does not yet exist.
/// </summary>
/// <remarks>
/// Which indexes are missing is determined when the job is created at host start; only the rebuild itself waits for
/// <see cref="Delay"/>.
/// </remarks>
internal sealed class IndexRebuildJob : RecurringBackgroundJobBase
{
    // Until the check has run once, the job stays schedulable so it can retry while the runtime is still
    // installing or upgrading. RunJobAsync retires the job by setting an infinite period afterwards.
    private static readonly TimeSpan _retryPeriod = TimeSpan.FromMinutes(1);

    private static readonly ServerRole[] _allServerRoles = Enum.GetValues<ServerRole>();

    private readonly IActiveIndexManager _activeIndexManager;
    private readonly IContentIndexingService _contentIndexingService;
    private readonly ILogger<IndexRebuildJob> _logger;
    private readonly IOriginProvider _originProvider;
    private readonly string[] _indexAliasesToRebuild;
    private int _hasRun;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexRebuildJob"/> class.
    /// </summary>
    /// <param name="examineManager">The manager used to check whether an index's active physical index already exists.</param>
    /// <param name="activeIndexManager">The manager used to resolve an index alias to its currently active physical index name.</param>
    /// <param name="contentIndexingService">The service used to trigger a rebuild of a missing index.</param>
    /// <param name="options">The options listing the registered content indexes to check on startup.</param>
    /// <param name="logger">The logger used to record which indexes are being rebuilt.</param>
    /// <param name="originProvider">The provider used to obtain the current server origin for the rebuild.</param>
    public IndexRebuildJob(
        IExamineManager examineManager,
        IActiveIndexManager activeIndexManager,
        IContentIndexingService contentIndexingService,
        IOptions<IndexOptions> options,
        ILogger<IndexRebuildJob> logger,
        IOriginProvider originProvider)
        : base(_retryPeriod)
    {
        _activeIndexManager = activeIndexManager;
        _contentIndexingService = contentIndexingService;
        _logger = logger;
        _originProvider = originProvider;

        // Determined up front rather than when the job runs: anything indexed during the startup delay creates the
        // physical index, which would then hide that it still needs a full rebuild.
        _indexAliasesToRebuild = options.Value.GetContentIndexRegistrations()
            .Select(registration => registration.IndexAlias)
            .Where(indexAlias => IsMissing(indexAlias, examineManager, activeIndexManager))
            .ToArray();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Gives the server a chance to finish starting up before potentially resource-intensive index rebuilds begin.
    /// </remarks>
    public override TimeSpan Delay => TimeSpan.FromMinutes(2);

    /// <summary>
    /// Gets the server roles on which this job runs.
    /// </summary>
    /// <remarks>
    /// Every server maintains its own local Lucene indexes, so the check must run on every server role, not just the
    /// scheduling publisher. <see cref="ServerRole.Unknown"/> is included deliberately: a server whose role has not
    /// been resolved yet still needs its own indexes.
    /// </remarks>
    public override ServerRole[] ServerRoles => _allServerRoles;

    /// <inheritdoc />
    /// <remarks>
    /// Only reached once the runtime is running, the server role matches and this is the main domain, so a single pass
    /// is enough - subsequent invocations are no-ops.
    /// </remarks>
    public override Task RunJobAsync(CancellationToken cancellationToken)
    {
        // Setting the period below does not take effect until the wait after this one, so the loop would otherwise
        // schedule a second pass - which would re-queue rebuilds that are still in flight and therefore not yet
        // visible as existing indexes.
        if (Interlocked.CompareExchange(ref _hasRun, 1, 0) != 0)
        {
            return Task.CompletedTask;
        }

        // Skip indexes already being rebuilt, for example from the backoffice during the startup delay.
        var indexAliasesToRebuild = _indexAliasesToRebuild
            .Where(indexAlias => _activeIndexManager.IsRebuilding(indexAlias) is false)
            .ToArray();

        _logger.LogInformation("Runtime is ready, rebuilding {IndexCount} index(es) missing at startup.", indexAliasesToRebuild.Length);

        var origin = _originProvider.GetCurrent();
        foreach (var indexAlias in indexAliasesToRebuild)
        {
            _contentIndexingService.Rebuild(indexAlias, origin);
        }

        // Retire the job so it doesn't run again, since the check has been performed once.
        Period = Timeout.InfiniteTimeSpan;
        _logger.LogInformation("Index rebuild check complete, no further runs are scheduled.");

        return Task.CompletedTask;
    }

    // An alias whose active physical index isn't a registered Examine index isn't rebuilt from here.
    private static bool IsMissing(string indexAlias, IExamineManager examineManager, IActiveIndexManager activeIndexManager)
        => examineManager.TryGetIndex(activeIndexManager.ResolveActiveIndexName(indexAlias), out IIndex? index)
           && index.IndexExists() is false;
}
