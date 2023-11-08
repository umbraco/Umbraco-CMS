using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Infrastructure.HostedServices;

public class WebhookFiring : RecurringHostedServiceBase
{
    public WebhookFiring(ILogger? logger)
        : base(logger, TimeSpan.FromMinutes(1), DefaultDelay)
    {
    }

    public override Task PerformExecuteAsync(object? state)
    {
        return Task.CompletedTask;
    }
}
