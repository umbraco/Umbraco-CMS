// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a single block's data in raw form.
/// </summary>
public class BlockItemData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockItemData" /> class.
    /// </summary>
    public BlockItemData()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockItemData" /> class.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="contentTypeKey">The content type key.</param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    [Obsolete("Use constructor that accepts GUID key instead. Will be removed in V18.")]
    public BlockItemData(Udi udi, Guid contentTypeKey, string contentTypeAlias)
        : this(
            (udi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(udi)),
            contentTypeKey,
            contentTypeAlias)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockItemData" /> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="contentTypeKey">The content type key.</param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    public BlockItemData(Guid key, Guid contentTypeKey, string contentTypeAlias)
    {
        ContentTypeAlias = contentTypeAlias;
        Key = key;
        Udi = new GuidUdi(Constants.UdiEntityType.Element, key);
        ContentTypeKey = contentTypeKey;
    }

    /// <summary>
    ///     Gets or sets the content type key.
    /// </summary>
    /// <value>
    ///     The content type key.
    /// </value>
    public Guid ContentTypeKey { get; set; }

    /// <summary>
    ///     Gets or sets the content type alias.
    /// </summary>
    /// <remarks>
    ///     Not serialized, manually set and used internally.
    /// </remarks>
    [JsonIgnore]
    public string ContentTypeAlias { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the UDI.
    /// </summary>
    /// <value>
    ///     The UDI.
    /// </value>
    [Obsolete("Use Key instead. Will be removed in V18.")]
    [JsonIgnore]
    public Udi? Udi { get; set; }

    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    /// <value>
    ///     The key.
    /// </value>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the property values.
    /// </summary>
    /// <value>
    ///     The property values.
    /// </value>
    public IList<BlockPropertyValue> Values { get; set; } = new List<BlockPropertyValue>();

    /// <summary>
    ///     Gets or sets the raw property values.
    /// </summary>
    /// <value>
    ///     The raw property values.
    /// </value>
    [Obsolete("Use Properties instead. Will be removed in V18.")]
    [JsonExtensionData]
    public Dictionary<string, object?> RawPropertyValues { get; set; } = new();
}
