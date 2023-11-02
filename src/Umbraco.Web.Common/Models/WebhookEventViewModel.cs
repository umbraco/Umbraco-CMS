using System.Runtime.Serialization;

namespace Umbraco.Cms.Web.Common.Models;

[DataContract]
public class WebhookEventViewModel
{
    [DataMember(Name = "eventName")]
    public string EventName { get; set; } = string.Empty;
}
