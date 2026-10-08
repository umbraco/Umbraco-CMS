// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Used for deserializing the block list layout.
/// </summary>
public class BlockListLayoutItem : BlockLayoutItemBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockListLayoutItem" /> class.
    /// </summary>
    public BlockListLayoutItem()
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockListLayoutItem" /> class.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    public BlockListLayoutItem(Guid contentKey)
        : base(contentKey)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockListLayoutItem" /> class.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    /// <param name="settingsKey">The settings key.</param>
    public BlockListLayoutItem(Guid contentKey, Guid settingsKey)
        : base(contentKey, settingsKey)
    {
    }
}
