// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Used for deserializing the block grid layout
/// </summary>
public class BlockGridLayoutItem : IBlockLayoutItem
{
    public Udi? ContentUdi { get; set; }

    public Udi? SettingsUdi { get; set; }

    public int? ColumnSpan { get; set; }

    public int? RowSpan { get; set; }

    public BlockGridLayoutAreaItem[] Areas { get; set; } = Array.Empty<BlockGridLayoutAreaItem>();

    public BlockGridLayoutItem()
    { }

    public BlockGridLayoutItem(Udi contentUdi)
        => ContentUdi = contentUdi;

    public BlockGridLayoutItem(Udi contentUdi, Udi settingsUdi)
        : this(contentUdi)
        => SettingsUdi = settingsUdi;
}
