using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.HostedServices;

public class WebhookLoggingCleanup : IRecurringBackgroundJob
{
    private readonly ILogger<WebhookLoggingCleanup> _logger;
    private readonly IOptionsMonitor<WebhookSettings> _webhookSettings;

    public WebhookLoggingCleanup(ILogger<WebhookLoggingCleanup> logger, IOptionsMonitor<WebhookSettings> webhookSettings)
    {
        _logger = logger;
        _webhookSettings = webhookSettings;
    }

    public TimeSpan Period => TimeSpan.FromDays(1);

    public TimeSpan Delay { get; } = TimeSpan.FromSeconds(20);

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }

    public Task RunJobAsync()
    {
        if (!_webhookSettings.CurrentValue.Enabled)
        {
            _logger.LogInformation("WebhookLoggingCleanup task will not run as it has been globally disabled via configuration");
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
