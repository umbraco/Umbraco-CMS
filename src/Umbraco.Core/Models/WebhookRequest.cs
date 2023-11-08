namespace Umbraco.Cms.Core.Models;

public class WebhookRequest
{
    public int Id { get; set; }

    public Guid WebhookKey { get; set; }

    public string EventName { get; set; } = string.Empty;

    public object? RequestObject { get; set; }

    public int RetryCount { get; set; }
}
