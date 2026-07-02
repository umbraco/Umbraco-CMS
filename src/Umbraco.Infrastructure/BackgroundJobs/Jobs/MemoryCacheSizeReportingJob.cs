// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
///     Periodically logs, at debug level, the approximate entry count of each in-memory cache that
///     scales with the size of the content tree, together with process-level memory totals.
/// </summary>
/// <remarks>
///     Intended as observability for memory usage during full-tree operations (reindexing, crawling the
///     published site). The per-cache counts are a trend/attribution signal — a count that climbs and
///     never falls indicates unbounded retention; the managed-heap and working-set totals give the
///     absolute memory picture. Runs on all servers because memory is per-process, and does nothing unless
///     debug logging is enabled for this job.
/// </remarks>
public class MemoryCacheSizeReportingJob : RecurringBackgroundJobBase
{
    private readonly IEnumerable<IMemoryCacheSizeReporter> _reporters;
    private readonly ILogger<MemoryCacheSizeReportingJob> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemoryCacheSizeReportingJob" /> class.
    /// </summary>
    /// <param name="reporters">The in-memory caches that report their size.</param>
    /// <param name="logger">The typed logger.</param>
    public MemoryCacheSizeReportingJob(
        IEnumerable<IMemoryCacheSizeReporter> reporters,
        ILogger<MemoryCacheSizeReportingJob> logger)
        : base(TimeSpan.FromMinutes(1))
    {
        _reporters = reporters;
        _logger = logger;
    }

    /// <summary>
    ///     Gets the server roles on which this job runs.
    /// </summary>
    /// <remarks>Runs on all servers, because the reported memory is per-process.</remarks>
    public override ServerRole[] ServerRoles => Enum.GetValues<ServerRole>();

    /// <inheritdoc />
    public override Task RunJobAsync(CancellationToken cancellationToken)
    {
        // Reporting is debug-only; skip the work entirely when debug logging is not enabled.
        if (_logger.IsEnabled(LogLevel.Debug) is false)
        {
            return Task.CompletedTask;
        }

        foreach (IMemoryCacheSizeReporter reporter in _reporters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            long? approximateBytes = reporter.GetApproximateBytes();
            if (approximateBytes is null)
            {
                _logger.LogDebug(
                    "In-memory cache size: {CacheName} = {EntryCount} entries (bytes: n/a — use a GC dump)",
                    reporter.CacheName,
                    reporter.GetApproximateCount());
            }
            else
            {
                _logger.LogDebug(
                    "In-memory cache size: {CacheName} = {EntryCount} entries (~{ApproximateBytes} bytes)",
                    reporter.CacheName,
                    reporter.GetApproximateCount(),
                    approximateBytes.Value);
            }
        }

        // The reporters above cover the L0 converted-content caches and the baseline structures. The
        // HybridCache L1 (Microsoft's in-process tier of ContentCacheNode entries, behind L0) does not
        // expose an entry count; capture it from a GC dump when a finer breakdown is needed. The process
        // totals below give the overall picture.
        _logger.LogDebug(
            "Process memory: managed heap {ManagedHeapBytes} bytes, working set {WorkingSetBytes} bytes",
            GC.GetTotalMemory(forceFullCollection: false),
            Environment.WorkingSet);

        return Task.CompletedTask;
    }
}
