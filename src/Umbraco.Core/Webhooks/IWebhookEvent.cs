namespace Umbraco.Cms.Core.Webhooks;

public interface IWebhookEvent
{
    string EventName { get; set; }

    string EventType { get; }

    string Alias { get; }
}
