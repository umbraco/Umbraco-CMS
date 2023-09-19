using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Web.Common.Models;

[DataContract]
public class WebhookViewModel
{
    [DataMember(Name = "key")]
    public Guid? Key { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; } = string.Empty;

    [DataMember(Name = "event")]
    public WebhookEvent[] Events { get; set; } = Array.Empty<WebhookEvent>();

    [DataMember(Name = "entityKeys")]
    public Guid[] EntityKeys { get; set; } = Array.Empty<Guid>();

    [DataMember(Name = "enabled")]
    public bool Enabled { get; set; }
}
