// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Used for deserializing the block grid layout.
/// </summary>
public class BlockGridLayoutItem : BlockLayoutItemBase
{
    /// <summary>
    ///     Gets or sets the number of columns this item should span.
    /// </summary>
    /// <value>
    ///     The column span.
    /// </value>
    public int? ColumnSpan { get; set; }

    /// <summary>
    ///     Gets or sets the number of rows this item should span.
    /// </summary>
    /// <value>
    ///     The row span.
    /// </value>
    public int? RowSpan { get; set; }

    /// <summary>
    ///     Gets or sets the areas within this layout item.
    /// </summary>
    /// <value>
    ///     The areas.
    /// </value>
    public BlockGridLayoutAreaItem[] Areas { get; set; } = Array.Empty<BlockGridLayoutAreaItem>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockGridLayoutItem" /> class.
    /// </summary>
    public BlockGridLayoutItem()
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockGridLayoutItem" /> class.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public BlockGridLayoutItem(Udi contentUdi)
        : base(contentUdi)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockGridLayoutItem" /> class.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    /// <param name="settingsUdi">The settings UDI.</param>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public BlockGridLayoutItem(Udi contentUdi, Udi settingsUdi)
        : base(contentUdi, settingsUdi)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockGridLayoutItem" /> class.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    public BlockGridLayoutItem(Guid contentKey)
        : base(contentKey)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockGridLayoutItem" /> class.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    /// <param name="settingsKey">The settings key.</param>
    public BlockGridLayoutItem(Guid contentKey, Guid settingsKey)
        : base(contentKey, settingsKey)
    {
    }

    /// <inheritdoc />
    public override bool ReferencesContent(Guid key)
        => ContentKey == key || Areas.Any(area => area.ContainsContent(key));

    /// <inheritdoc />
    public override bool ReferencesSetting(Guid key)
        => SettingsKey == key || Areas.Any(area => area.ContainsSetting(key));
}
