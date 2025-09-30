// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Used for deserializing the rich text block layouts
/// </summary>
public class RichTextBlockLayoutItem : BlockLayoutItemBase
{
    public RichTextBlockLayoutItem()
    { }

    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public RichTextBlockLayoutItem(Udi contentUdi)
        : base(contentUdi)
    {
    }

    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public RichTextBlockLayoutItem(Udi contentUdi, Udi settingsUdi)
        : base(contentUdi, settingsUdi)
    {
    }

    public RichTextBlockLayoutItem(Guid contentKey)
        : base(contentKey)
    {
    }

    public RichTextBlockLayoutItem(Guid contentKey, Guid settingsKey)
        : base(contentKey, settingsKey)
    {
    }
}
