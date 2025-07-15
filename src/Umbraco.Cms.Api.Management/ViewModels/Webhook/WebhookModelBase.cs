using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

public class WebhookModelBase
{
    public bool Enabled { get; set; } = true;

    public string? Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public string Url { get; set; } = string.Empty;

    public Guid[] ContentTypeKeys { get; set; } = Array.Empty<Guid>();

    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}
