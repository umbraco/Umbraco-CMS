// Copyright (c) Umbraco.
// See LICENSE for more details.

using Newtonsoft.Json;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks
{
    /// <summary>
    /// Used for deserializing the block grid layout
    /// </summary>
    public class BlockGridLayoutItem : IBlockLayoutItem
    {
        [JsonProperty("contentUdi", Required = Required.Always)]
        [JsonConverter(typeof(UdiJsonConverter))]
        public Udi? ContentUdi { get; set; }

        [JsonProperty("settingsUdi", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(UdiJsonConverter))]
        public Udi? SettingsUdi { get; set; }

        [JsonProperty("columnSpan", NullValueHandling = NullValueHandling.Ignore)]
        public int? ColumnSpan { get; set; }

        [JsonProperty("rowSpan", NullValueHandling = NullValueHandling.Ignore)]
        public int? RowSpan { get; set; }

        [JsonProperty("areaGridColumns", NullValueHandling = NullValueHandling.Ignore)]
        public int? AreaGridColumns { get; set; }

        [JsonProperty("areas", NullValueHandling = NullValueHandling.Ignore)]
        public BlockGridLayoutAreaItem[] Areas { get; set; } = Array.Empty<BlockGridLayoutAreaItem>();

        /*
        [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
        public JEnumerable<JToken> Children { get; set; }
        */
    }

    public class BlockGridLayoutAreaItem
    {
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public Guid Key { get; set; } = Guid.Empty;

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public BlockGridLayoutItem[] Items { get; set; } = Array.Empty<BlockGridLayoutItem>();
    }
}
