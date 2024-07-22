// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Used for deserializing the block list layout
/// </summary>
public class BlockListLayoutItem : IBlockLayoutItem
{
    [Obsolete("Use ContentKey instead. Will be removed in V18.")]
    public Udi? ContentUdi { get; set; }

    [Obsolete("Use SettingsKey instead. Will be removed in V18.")]
    public Udi? SettingsUdi { get; set; }

    public Guid ContentKey { get; set; }

    public Guid? SettingsKey { get; set; }

    public BlockListLayoutItem()
    { }

    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public BlockListLayoutItem(Udi contentUdi)
        : this((contentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(contentUdi)))
    {
    }

    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public BlockListLayoutItem(Udi contentUdi, Udi settingsUdi)
        : this(
            (contentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(contentUdi)),
            (settingsUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(settingsUdi)))
    {
    }

    public BlockListLayoutItem(Guid contentKey)
    {
        ContentKey = contentKey;
        ContentUdi = new GuidUdi(Constants.UdiEntityType.Element, contentKey);
    }

    public BlockListLayoutItem(Guid contentKey, Guid settingsKey)
        : this(contentKey)
    {
        SettingsKey = settingsKey;
        SettingsUdi = new GuidUdi(Constants.UdiEntityType.Element, settingsKey);
    }
}
