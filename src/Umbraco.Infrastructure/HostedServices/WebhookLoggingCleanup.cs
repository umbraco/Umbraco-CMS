using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HostedServices;

public class WebhookLoggingCleanup : IRecurringBackgroundJob
{
    private readonly ILogger<WebhookLoggingCleanup> _logger;
    private readonly WebhookSettings _webhookSettings;
    private readonly IWebhookLogRepository _webhookLogRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public WebhookLoggingCleanup(ILogger<WebhookLoggingCleanup> logger, IOptionsMonitor<WebhookSettings> webhookSettings, IWebhookLogRepository webhookLogRepository, ICoreScopeProvider coreScopeProvider)
    {
        _logger = logger;
        _webhookSettings = webhookSettings.CurrentValue;
        _webhookLogRepository = webhookLogRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    public TimeSpan Period => TimeSpan.FromDays(1);

    public TimeSpan Delay { get; } = TimeSpan.FromSeconds(20);

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }

    public async Task RunJobAsync()
    {
        if (!_webhookSettings.Enabled)
        {
            _logger.LogInformation("WebhookLoggingCleanup task will not run as it has been globally disabled via configuration");
            return;
        }

        IEnumerable<WebhookLog> webhookLogs = await _webhookLogRepository.GetOlderThanDate(DateTime.Now - _webhookSettings.Period);

        foreach (IEnumerable<WebhookLog> group in webhookLogs.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            scope.WriteLock(Constants.Locks.WebhookLogs);
            await _webhookLogRepository.DeleteByIds(group.Select(x => x.Id).ToArray());

            scope.Complete();
        }
    }
}
