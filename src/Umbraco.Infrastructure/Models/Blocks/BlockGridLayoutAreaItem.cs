using System.ComponentModel;
using Newtonsoft.Json;

namespace Umbraco.Cms.Core.Models.Blocks;

[EditorBrowsable(EditorBrowsableState.Never)] // TODO: Remove this for V11/V10.4
public class BlockGridLayoutAreaItem
{
    [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
    public Guid Key { get; set; } = Guid.Empty;

    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    public BlockGridLayoutItem[] Items { get; set; } = Array.Empty<BlockGridLayoutItem>();
}
