// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Used for deserializing the block grid layout
/// </summary>
public class BlockGridLayoutItem : BlockLayoutItemBase
{
    public int? ColumnSpan { get; set; }

    public int? RowSpan { get; set; }

    public BlockGridLayoutAreaItem[] Areas { get; set; } = Array.Empty<BlockGridLayoutAreaItem>();

    public BlockGridLayoutItem()
    { }

    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public BlockGridLayoutItem(Udi contentUdi)
        : base(contentUdi)
    {
    }

    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public BlockGridLayoutItem(Udi contentUdi, Udi settingsUdi)
        : base(contentUdi, settingsUdi)
    {
    }

    public BlockGridLayoutItem(Guid contentKey)
        : base(contentKey)
    {
    }

    public BlockGridLayoutItem(Guid contentKey, Guid settingsKey)
        : base(contentKey, settingsKey)
    {
    }
}
