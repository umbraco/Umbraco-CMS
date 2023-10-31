namespace Umbraco.Cms.Core.Models;

public class WebhookResponseModel
{
    public HttpResponseMessage? HttpResponseMessage { get; set; }

    public int RetryCount { get; set; }
}
