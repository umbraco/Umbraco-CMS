using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.Core.Manifest
{
    public class ManifestDashboardTab
    {
        public ManifestDashboardTab()
        {
            Controls = new List<ManifestDashboardControl>();
            Index = int.MaxValue; //default so we can check if this value has been explicitly set
        }

        [JsonProperty("controls")]
        public List<ManifestDashboardControl> Controls { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }
    }
}