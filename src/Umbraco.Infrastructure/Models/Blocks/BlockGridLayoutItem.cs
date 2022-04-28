using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks
{
    /// <summary>
    /// Used for deserializing the block grid layout
    /// </summary>
    public class BlockGridLayoutItem
    {
        [JsonProperty("contentUdi", Required = Required.Always)]
        [JsonConverter(typeof(UdiJsonConverter))]
        public Udi ContentUdi { get; set; }

        [JsonProperty("settingsUdi", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(UdiJsonConverter))]
        public Udi SettingsUdi { get; set; }

        /*
        [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
        public JEnumerable<JToken> Children { get; set; }
        */
    }
}
