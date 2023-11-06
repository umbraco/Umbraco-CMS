namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

public class WebhookModelBase
{
    public bool Enabled { get; set; } = true;

    public string Name { get; set; } = string.Empty;

    public string? Events { get; set; }

    public string? Url { get; set; }

    public string? Types { get; set; }
}
