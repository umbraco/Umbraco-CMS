// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a layout item for the Block List editor.
/// </summary>
/// <seealso cref="IBlockReference{IPublishedElement}" />
[DataContract(Name = "block", Namespace = "")]
public class RichTextBlockItem : IBlockReference<IPublishedElement, IPublishedElement>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextBlockItem" /> class.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    /// <param name="content">The content.</param>
    /// <param name="settingsUdi">The settings UDI.</param>
    /// <param name="settings">The settings.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     contentUdi
    ///     or
    ///     content
    /// </exception>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public RichTextBlockItem(Udi contentUdi, IPublishedElement content, Udi settingsUdi, IPublishedElement settings)
        : this(
            (contentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(contentUdi)),
            content,
            (settingsUdi as GuidUdi)?.Guid,
            settings)
    {
    }

    public RichTextBlockItem(Guid contentKey, IPublishedElement content, Guid? settingsKey, IPublishedElement? settings)
    {
        ContentKey = contentKey;
        ContentUdi = new GuidUdi(Constants.UdiEntityType.Element, contentKey);
        Content = content ?? throw new ArgumentNullException(nameof(content));
        SettingsKey = settingsKey;
        SettingsUdi = settingsKey.HasValue
            ? new GuidUdi(Constants.UdiEntityType.Element, settingsKey.Value)
            : null;
        Settings = settings;
    }

    public Guid ContentKey { get; set; }

    public Guid? SettingsKey { get; set; }

    /// <summary>
    ///     Gets the content.
    /// </summary>
    /// <value>
    ///     The content.
    /// </value>
    public IPublishedElement Content { get; }

    /// <summary>
    ///     Gets the settings UDI.
    /// </summary>
    /// <value>
    ///     The settings UDI.
    /// </value>
    [Obsolete("Use SettingsKey instead. Will be removed in V18.")]
    public Udi? SettingsUdi { get; }

    /// <summary>
    ///     Gets the content UDI.
    /// </summary>
    /// <value>
    ///     The content UDI.
    /// </value>
    [Obsolete("Use ContentKey instead. Will be removed in V18.")]
    public Udi ContentUdi { get; }

    /// <summary>
    ///     Gets the settings.
    /// </summary>
    /// <value>
    ///     The settings.
    /// </value>
    public IPublishedElement? Settings { get; }
}

/// <summary>
///     Represents a layout item with a generic content type for the Block List editor.
/// </summary>
/// <typeparam name="T">The type of the content.</typeparam>
/// <seealso cref="IBlockReference{IPublishedElement}" />
public class RichTextBlockItem<T> : RichTextBlockItem
    where T : IPublishedElement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextBlockItem{T}" /> class.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    /// <param name="content">The content.</param>
    /// <param name="settingsUdi">The settings UDI.</param>
    /// <param name="settings">The settings.</param>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public RichTextBlockItem(Udi contentUdi, T content, Udi settingsUdi, IPublishedElement settings)
        : base(contentUdi, content, settingsUdi, settings) =>
        Content = content;

    public RichTextBlockItem(Guid contentKey, T content, Guid? settingsKey, IPublishedElement? settings)
        : base(contentKey, content, settingsKey, settings) =>
        Content = content;

    /// <summary>
    ///     Gets the content.
    /// </summary>
    /// <value>
    ///     The content.
    /// </value>
    public new T Content { get; }
}

/// <summary>
///     Represents a layout item with generic content and settings types for the Block List editor.
/// </summary>
/// <typeparam name="TContent">The type of the content.</typeparam>
/// <typeparam name="TSettings">The type of the settings.</typeparam>
/// <seealso cref="IBlockReference{IPublishedElement}" />
public class RichTextBlockItem<TContent, TSettings> : RichTextBlockItem<TContent>
    where TContent : IPublishedElement
    where TSettings : IPublishedElement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextBlockItem{TContent, TSettings}" /> class.
    /// </summary>
    /// <param name="contentUdi">The content udi.</param>
    /// <param name="content">The content.</param>
    /// <param name="settingsUdi">The settings udi.</param>
    /// <param name="settings">The settings.</param>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public RichTextBlockItem(Udi contentUdi, TContent content, Udi settingsUdi, TSettings settings)
        : base(contentUdi, content, settingsUdi, settings) =>
        Settings = settings;

    public RichTextBlockItem(Guid contentKey, TContent content, Guid? settingsKey, TSettings? settings)
        : base(contentKey, content, settingsKey, settings) =>
        Settings = settings;

    /// <summary>
    ///     Gets the settings.
    /// </summary>
    /// <value>
    ///     The settings.
    /// </value>
    public new TSettings? Settings { get; }
}
