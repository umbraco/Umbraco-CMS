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

    [Obsolete("Use constructor that accepts GUID key instead. Will be removed in V18.")]
    public BlockItemData(Udi udi, Guid contentTypeKey, string contentTypeAlias)
        : this(
            (udi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(udi)),
            contentTypeKey,
            contentTypeAlias)
    {
    }

    public BlockItemData(Guid key, Guid contentTypeKey, string contentTypeAlias)
    {
        ContentTypeAlias = contentTypeAlias;
        Key = key;
        Udi = new GuidUdi(Constants.UdiEntityType.Element, key);
        ContentTypeKey = contentTypeKey;
    }

    public Guid ContentTypeKey { get; set; }

    /// <summary>
    ///     not serialized, manually set and used during internally
    /// </summary>
    [JsonIgnore]
    public string ContentTypeAlias { get; set; } = string.Empty;

    [Obsolete("Use Key instead. Will be removed in V18.")]
    public Udi? Udi { get; set; }

    public Guid Key { get; set; }

    public IList<BlockPropertyValue> Values { get; set; } = new List<BlockPropertyValue>();

    [Obsolete("Use Properties instead. Will be removed in V18.")]
    [JsonExtensionData]
    public Dictionary<string, object?> RawPropertyValues { get; set; } = new();
}
