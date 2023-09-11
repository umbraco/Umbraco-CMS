using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Web.Common.Models;

public class WebhookViewModel
{
    public string Url { get; set; } = string.Empty;

    public WebhookEvent Event { get; set; }

    public Guid[] EntityKeys { get; set; } = Array.Empty<Guid>();
}
