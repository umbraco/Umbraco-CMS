using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Add Umbraco background jobs
    /// </summary>
    public static IUmbracoBuilder AddBackgroundJobs(this IUmbracoBuilder builder)
    {
        // Idempotency check using a private marker class
        if (builder.Services.Any(s => s.ServiceType == typeof(BackgroundJobsMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<BackgroundJobsMarker>();

        // Add background jobs
        builder.Services.AddRecurringBackgroundJob<TempFileCleanupJob>();
        builder.Services.AddRecurringBackgroundJob<InstructionProcessJob>();
        builder.Services.AddRecurringBackgroundJob<TouchServerJob>();
        builder.Services.AddRecurringBackgroundJob<ReportSiteJob>();

        builder.Services.AddSingleton<IDistributedBackgroundJob, WebhookFiring>();
        builder.Services.AddSingleton<IDistributedBackgroundJob, ContentVersionCleanupJob>();
        builder.Services.AddSingleton<IDistributedBackgroundJob, HealthCheckNotifierJob>();
        builder.Services.AddSingleton<IDistributedBackgroundJob, LogScrubberJob>();
        builder.Services.AddSingleton<IDistributedBackgroundJob, ScheduledPublishingJob>();
        builder.Services.AddSingleton<IDistributedBackgroundJob, TemporaryFileCleanupJob>();
        builder.Services.AddSingleton<IDistributedBackgroundJob, WebhookLoggingCleanup>();
        builder.Services.AddSingleton<IDistributedBackgroundJob, CacheInstructionsPruningJob>();
        builder.Services.AddSingleton<IDistributedBackgroundJob, LongRunningOperationsCleanupJob>();
        builder.Services.AddHostedService<DistributedBackgroundJobHostedService>();

        builder.Services.AddSingleton(RecurringBackgroundJobHostedService.CreateHostedServiceFactory);
        builder.Services.AddHostedService<RecurringBackgroundJobHostedServiceRunner>();
        builder.Services.AddHostedService<QueuedHostedService>();
        builder.AddNotificationAsyncHandler<PostRuntimePremigrationsUpgradeNotification, NavigationInitializationNotificationHandler>();
        builder.AddNotificationAsyncHandler<PostRuntimePremigrationsUpgradeNotification, PublishStatusInitializationNotificationHandler>();

        return builder;
    }

    /// <summary>
    /// Marker class to ensure AddBackgroundJobs is only called once.
    /// </summary>
    private sealed class BackgroundJobsMarker
    {
    }
}
