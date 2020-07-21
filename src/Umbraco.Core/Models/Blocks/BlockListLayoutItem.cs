using Newtonsoft.Json;
using Umbraco.Core.Serialization;
using static Umbraco.Core.Models.Blocks.BlockEditorData;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// Used for deserializing the block list layout
    /// </summary>
    public class BlockListLayoutItem
    {
        [JsonProperty("settings")]
        public BlockItemData Settings { get; set; }

        [JsonProperty("udi")]
        [JsonConverter(typeof(UdiJsonConverter))]
        public Udi Udi { get; set; }
    }
}
