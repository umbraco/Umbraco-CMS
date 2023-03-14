// Copyright (c) Umbraco.
// See LICENSE for more details.

using Newtonsoft.Json;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Used for deserializing the block list layout
/// </summary>
public class BlockListLayoutItem : IBlockLayoutItem
{
    [JsonProperty("contentUdi", Required = Required.Always)]
    [JsonConverter(typeof(UdiJsonConverter))]
    public Udi? ContentUdi { get; set; }

    [JsonProperty("settingsUdi", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(UdiJsonConverter))]
    public Udi? SettingsUdi { get; set; }
}
