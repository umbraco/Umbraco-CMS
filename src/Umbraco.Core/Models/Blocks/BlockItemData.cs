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
    /// <param name="key">The key.</param>
    /// <param name="contentTypeKey">The content type key.</param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    public BlockItemData(Guid key, Guid contentTypeKey, string contentTypeAlias)
    {
        ContentTypeAlias = contentTypeAlias;
        Key = key;
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
}
