using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.Core.Manifest
{
    public class ManifestDashboardSection
    {
        public ManifestDashboardSection()
        {
            Areas = new List<string>();
            Tabs = new Dictionary<string, ManifestDashboardTab>();
        }

        [JsonProperty("areas")]
        public List<string> Areas { get; set; }

        [JsonProperty("tabs")]
        public IDictionary<string, ManifestDashboardTab> Tabs { get; set; }
    }
}