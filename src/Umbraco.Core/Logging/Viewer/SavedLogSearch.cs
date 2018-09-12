using Newtonsoft.Json;

namespace Umbraco.Core.Logging.Viewer
{
    public class SavedLogSearch
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }
    }
}
