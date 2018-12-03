using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.Core.Manifest
{
    public class ManifestDashboard
    {
        public ManifestDashboard()
        {
            Name = string.Empty;
            Alias = string.Empty;
            Weight = int.MaxValue; //default so we can check if this value has been explicitly set
            View = string.Empty;
            Sections = new List<string>();
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("aias")]
        public string Alias { get; set; }

        [JsonProperty("weight")]
        public int Weight { get; set; }

        [JsonProperty("view")]
        public string View { get; set; }

        [JsonProperty("sections")]
        public List<string> Sections { get; set; }
        
    }
}
