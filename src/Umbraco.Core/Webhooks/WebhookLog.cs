namespace Umbraco.Cms.Core.Webhooks;

public class WebhookLog
{
    public int Id { get; set; }

    public Guid Key { get; set; }

    public string Url { get; set; } = string.Empty;

    public string StatusCode { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string EventName { get; set; } = string.Empty;

    public int RetryCount { get; set; }

    public string RequestBody { get; set; } = string.Empty;

    public string ResponseBody { get; set; } = string.Empty;
}
