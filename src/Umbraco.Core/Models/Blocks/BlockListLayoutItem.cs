using Newtonsoft.Json;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// Used for deserializing the block list layout
    /// </summary>
    public class BlockListLayoutItem
    {
        [JsonProperty("contentUdi", Required = Required.Always)]
        [JsonConverter(typeof(UdiJsonConverter))]
        public Udi ContentUdi { get; set; }

        [JsonProperty("settingsUdi", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(UdiJsonConverter))]
        public Udi SettingsUdi { get; set; }
    }
}
