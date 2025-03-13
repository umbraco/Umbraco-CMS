using System.Runtime.Serialization;

namespace Umbraco.Cms.Web.Common.Models;

[DataContract]
public class WebhookViewModel
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "key")]
    public Guid? Key { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; } = string.Empty;

    [DataMember(Name = "events")]
    public WebhookEventViewModel[] Events { get; set; } = Array.Empty<WebhookEventViewModel>();

    [DataMember(Name = "contentTypeKeys")]
    public Guid[] ContentTypeKeys { get; set; } = Array.Empty<Guid>();

    [DataMember(Name = "enabled")]
    public bool Enabled { get; set; }

    [DataMember(Name = "headers")]
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}
