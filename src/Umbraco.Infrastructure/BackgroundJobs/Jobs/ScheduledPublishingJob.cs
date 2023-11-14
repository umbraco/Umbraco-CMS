// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
///     Hosted service implementation for scheduled publishing feature.
/// </summary>
/// <remarks>
///     Runs only on non-replica servers.
/// </remarks>
public class ScheduledPublishingJob : IRecurringBackgroundJob
{
    public TimeSpan Period { get => TimeSpan.FromMinutes(1); }
    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }


    private readonly IContentService _contentService;
    private readonly ILogger<ScheduledPublishingJob> _logger;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IServerMessenger _serverMessenger;
    private readonly IUmbracoContextFactory _umbracoContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScheduledPublishingJob" /> class.
    /// </summary>
    public ScheduledPublishingJob(
        IContentService contentService,
        IUmbracoContextFactory umbracoContextFactory,
        ILogger<ScheduledPublishingJob> logger,
        IServerMessenger serverMessenger,
        ICoreScopeProvider scopeProvider)
    {
        _contentService = contentService;
        _umbracoContextFactory = umbracoContextFactory;
        _logger = logger;
        _serverMessenger = serverMessenger;
        _scopeProvider = scopeProvider;
    }

    public Task RunJobAsync()
    {
        if (Suspendable.ScheduledPublishing.CanRun == false)
        {
            return Task.CompletedTask;
        }

        try
        {
            // Ensure we run with an UmbracoContext, because this will run in a background task,
            // and developers may be using the UmbracoContext in the event handlers.

            // TODO: or maybe not, CacheRefresherComponent already ensures a context when handling events
            // - UmbracoContext 'current' needs to be refactored and cleaned up
            // - batched messenger should not depend on a current HttpContext
            //    but then what should be its "scope"? could we attach it to scopes?
            // - and we should definitively *not* have to flush it here (should be auto)
            using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
            using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

            /* We used to assume that there will never be two instances running concurrently where (IsMainDom && ServerRole == SchedulingPublisher)
             * However this is possible during an azure deployment slot swap for the SchedulingPublisher instance when trying to achieve zero downtime deployments.
             * If we take a distributed write lock, we are certain that the multiple instances of the job will not run in parallel.
             * It's possible that during the swapping process we may run this job more frequently than intended but this is not of great concern and it's
             * only until the old SchedulingPublisher shuts down. */
            scope.EagerWriteLock(Constants.Locks.ScheduledPublishing);
            try
            {
                // Run
                IEnumerable<PublishResult> result = _contentService.PerformScheduledPublish(DateTime.Now);
                foreach (IGrouping<PublishResultType, PublishResult> grouped in result.GroupBy(x => x.Result))
                {
                    _logger.LogInformation(
                        "Scheduled publishing result: '{StatusCount}' items with status {Status}",
                        grouped.Count(),
                        grouped.Key);
                }
            }
            finally
            {
                // If running on a temp context, we have to flush the messenger
                if (contextReference.IsRoot)
                {
                    _serverMessenger.SendMessages();
                }
            }
        }
        catch (Exception ex)
        {
            // important to catch *everything* to ensure the task repeats
            _logger.LogError(ex, "Failed.");
        }

        return Task.CompletedTask;
    }
}
