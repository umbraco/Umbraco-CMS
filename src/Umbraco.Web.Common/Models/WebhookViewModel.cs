using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Web.Common.Models;

[DataContract]
public class WebhookViewModel
{
    [DataMember(Name = "url")]
    public string Url { get; set; } = string.Empty;

    [DataMember(Name = "event")]
    [JsonConverter(typeof(StringEnumConverter))]
    public WebhookEvent Event { get; set; }

    [DataMember(Name = "entityKeys")]
    public Guid[] EntityKeys { get; set; } = Array.Empty<Guid>();
}
