namespace Umbraco.Cms.Core.Models;

public class WebhookRequest
{
    public int Id { get; set; }

    public Guid WebhookKey { get; set; }

    public string EventAlias { get; set; } = string.Empty;

    public string? RequestObject { get; set; }

    public int RetryCount { get; set; }
}
