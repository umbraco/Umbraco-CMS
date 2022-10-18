using Newtonsoft.Json;

namespace Umbraco.Cms.Core.Models.Blocks;

public class BlockGridLayoutAreaItem
{
    [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
    public Guid Key { get; set; } = Guid.Empty;

    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    public BlockGridLayoutItem[] Items { get; set; } = Array.Empty<BlockGridLayoutItem>();
}
