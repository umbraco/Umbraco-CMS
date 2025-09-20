namespace Umbraco.Cms.Core.Webhooks;

public interface IWebhookEvent
{
    string EventName { get; }

    string EventType { get; }

    string Alias { get; }
}
