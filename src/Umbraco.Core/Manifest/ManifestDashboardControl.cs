using Newtonsoft.Json;

namespace Umbraco.Core.Manifest
{
    public class ManifestDashboardControl
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }
    }
}