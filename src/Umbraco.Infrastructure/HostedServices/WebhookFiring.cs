using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HostedServices;

public class WebhookFiring : RecurringHostedServiceBase
{
    private readonly ILogger<WebhookFiring> _logger;
    private readonly IRuntimeState _runtimeState;
    private readonly IWebhookRequestService _webhookRequestService;

    public WebhookFiring(ILogger<WebhookFiring> logger, IRuntimeState runtimeState, IWebhookRequestService webhookRequestService)
        : base(logger, TimeSpan.FromMinutes(1), DefaultDelay)
    {
        _logger = logger;
        _runtimeState = runtimeState;
        _webhookRequestService = webhookRequestService;
    }

    public override Task PerformExecuteAsync(object? state)
    {
        // Do NOT run publishing if not properly running
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Does not run if run level is not Run.");
            }

            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
