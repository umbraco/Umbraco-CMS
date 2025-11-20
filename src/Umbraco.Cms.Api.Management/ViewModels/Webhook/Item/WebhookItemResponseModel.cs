using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Webhook.Item;

public class WebhookItemResponseModel : ItemResponseModelBase
{
    public bool Enabled { get; set; } = true;

    public string Name { get; set; } = string.Empty;

    public string Events { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Types { get; set; } = string.Empty;
}
