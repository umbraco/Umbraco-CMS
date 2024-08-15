// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a single block's data in raw form
/// </summary>
public class BlockItemData
{
    public BlockItemData()
    {
    }

    public BlockItemData(Udi udi, Guid contentTypeKey, string contentTypeAlias)
    {
        ContentTypeAlias = contentTypeAlias;
        Udi = udi;
        ContentTypeKey = contentTypeKey;
    }

    public Guid ContentTypeKey { get; set; }

    /// <summary>
    ///     not serialized, manually set and used during internally
    /// </summary>
    [JsonIgnore]
    public string ContentTypeAlias { get; set; } = string.Empty;

    public Udi? Udi { get; set; }

    [JsonIgnore]
    public Guid Key => Udi is not null ? ((GuidUdi)Udi).Guid : throw new InvalidOperationException("No Udi assigned");

    public IList<BlockPropertyValue> Values { get; set; } = new List<BlockPropertyValue>();

    [Obsolete("Use Properties instead. Will be removed in V17.")]
    [JsonExtensionData]
    public Dictionary<string, object?> RawPropertyValues { get; set; } = new();
}
