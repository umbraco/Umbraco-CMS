using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// Daily background job that removes all webhook log data older than x days as defined by <see cref="WebhookSettings.KeepLogsForDays"/>
/// </summary>
public class WebhookLoggingCleanup : IDistributedBackgroundJob
{
    private readonly ILogger<WebhookLoggingCleanup> _logger;
    private readonly WebhookSettings _webhookSettings;
    private readonly IWebhookLogRepository _webhookLogRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookLoggingCleanup"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="webhookSettings"></param>
    /// <param name="webhookLogRepository"></param>
    /// <param name="coreScopeProvider"></param>
    public WebhookLoggingCleanup(ILogger<WebhookLoggingCleanup> logger, IOptionsMonitor<WebhookSettings> webhookSettings, IWebhookLogRepository webhookLogRepository, ICoreScopeProvider coreScopeProvider)
    {
        _logger = logger;
        _webhookSettings = webhookSettings.CurrentValue;
        _webhookLogRepository = webhookLogRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    /// <inheritdoc />
    public string Name => "WebhookLoggingCleanup";

    /// <inheritdoc />
    public TimeSpan Period => TimeSpan.FromDays(1);

    /// <inheritdoc />
    public async Task RunJobAsync()
    {
        if (_webhookSettings.EnableLoggingCleanup is false)
        {
            _logger.LogInformation("WebhookLoggingCleanup task will not run as it has been globally disabled via configuration");
            return;
        }

        IEnumerable<WebhookLog> webhookLogs;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            scope.ReadLock(Constants.Locks.WebhookLogs);
            webhookLogs = await _webhookLogRepository.GetOlderThanDate(DateTime.UtcNow - TimeSpan.FromDays(_webhookSettings.KeepLogsForDays));
            scope.Complete();
        }

        foreach (IEnumerable<WebhookLog> group in webhookLogs.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            scope.WriteLock(Constants.Locks.WebhookLogs);
            await _webhookLogRepository.DeleteByIds(group.Select(x => x.Id).ToArray());

            scope.Complete();
        }
    }
}
