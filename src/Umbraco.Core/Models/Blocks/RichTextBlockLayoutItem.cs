// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Used for deserializing the rich text block layouts
/// </summary>
public class RichTextBlockLayoutItem : IBlockLayoutItem
{
    public Udi? ContentUdi { get; set; }

    public Udi? SettingsUdi { get; set; }

    public RichTextBlockLayoutItem()
    { }

    public RichTextBlockLayoutItem(Udi contentUdi)
        => ContentUdi = contentUdi;

    public RichTextBlockLayoutItem(Udi contentUdi, Udi settingsUdi)
        : this(contentUdi)
        => SettingsUdi = settingsUdi;
}
