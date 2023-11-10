using System.Runtime.Serialization;

namespace Umbraco.Cms.Web.Common.Models;

[DataContract]
public class WebhookLogViewModel
{
    [DataMember(Name = "key")]
    public Guid Key { get; set; }

    [DataMember(Name = "webhookKey")]
    public Guid WebhookKey { get; set; }

    [DataMember(Name = "statusCode")]
    public string StatusCode { get; set; } = string.Empty;

    [DataMember(Name = "date")]
    public DateTime Date { get; set; }

    [DataMember(Name = "eventAlias")]
    public string EventAlias { get; set; } = string.Empty;

    [DataMember(Name = "url")]
    public string Url { get; set; } = string.Empty;

    [DataMember(Name = "retryCount")]
    public int RetryCount { get; set; }

    [DataMember(Name = "requestHeaders")]
    public string RequestHeaders { get; set; } = string.Empty;

    [DataMember(Name = "requestBody")]
    public string RequestBody { get; set; } = string.Empty;

    [DataMember(Name = "responseHeaders")]
    public string ResponseHeaders { get; set; } = string.Empty;

    [DataMember(Name = "responseBody")]
    public string ResponseBody { get; set; } = string.Empty;
}
