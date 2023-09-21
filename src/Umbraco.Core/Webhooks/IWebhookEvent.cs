namespace Umbraco.Cms.Core.Webhooks;

public interface IWebhookEvent
{
    string EventName { get; }
}
