using System.Text.Json.Serialization;

namespace Umbraco.Configuration.Models
{
    public class ActiveDirectorySettings
    {
        [JsonPropertyName("Domain")]
        public string ActiveDirectoryDomain { get; set; }
    }
}
