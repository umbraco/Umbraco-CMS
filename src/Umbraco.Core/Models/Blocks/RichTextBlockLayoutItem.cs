// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Used for deserializing the rich text block layouts.
/// </summary>
public class RichTextBlockLayoutItem : BlockLayoutItemBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextBlockLayoutItem" /> class.
    /// </summary>
    public RichTextBlockLayoutItem()
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextBlockLayoutItem" /> class.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public RichTextBlockLayoutItem(Udi contentUdi)
        : base(contentUdi)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextBlockLayoutItem" /> class.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    /// <param name="settingsUdi">The settings UDI.</param>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public RichTextBlockLayoutItem(Udi contentUdi, Udi settingsUdi)
        : base(contentUdi, settingsUdi)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextBlockLayoutItem" /> class.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    public RichTextBlockLayoutItem(Guid contentKey)
        : base(contentKey)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextBlockLayoutItem" /> class.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    /// <param name="settingsKey">The settings key.</param>
    public RichTextBlockLayoutItem(Guid contentKey, Guid settingsKey)
        : base(contentKey, settingsKey)
    {
    }
}
