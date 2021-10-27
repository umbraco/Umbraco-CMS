using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Blocks
{
    public class BlockValue
    {
        [JsonProperty("layout")]
        public IDictionary<string, JToken> Layout { get; set; }

        [JsonProperty("contentData")]
        public List<BlockItemData> ContentData { get; set; } = new List<BlockItemData>();

        [JsonProperty("settingsData")]
        public List<BlockItemData> SettingsData { get; set; } = new List<BlockItemData>();
    }
}
