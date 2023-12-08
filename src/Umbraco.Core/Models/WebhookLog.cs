namespace Umbraco.Cms.Core.Models;

public class WebhookLog
{
    public int Id { get; set; }

    public Guid WebhookKey { get; set; }

    public Guid Key { get; set; }

    public string Url { get; set; } = string.Empty;

    public string StatusCode { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string EventAlias { get; set; } = string.Empty;

    public int RetryCount { get; set; }

    public string RequestHeaders { get; set; } = string.Empty;

    public string? RequestBody { get; set; } = string.Empty;

    public string ResponseHeaders { get; set; } = string.Empty;

    public string ResponseBody { get; set; } = string.Empty;
}
