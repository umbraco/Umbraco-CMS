using Newtonsoft.Json;

namespace Umbraco.Cms.Core.Logging.Viewer;

public class SavedLogSearch
{
    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("query")]
    public required string Query { get; set; }
}
