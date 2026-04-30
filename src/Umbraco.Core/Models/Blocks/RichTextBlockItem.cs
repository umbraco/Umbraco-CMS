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
    /// <param name="contentKey">The content key.</param>
    /// <param name="content">The content.</param>
    /// <param name="settingsKey">The settings key.</param>
    /// <param name="settings">The settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public RichTextBlockItem(Guid contentKey, IPublishedElement content, Guid? settingsKey, IPublishedElement? settings)
    {
        ContentKey = contentKey;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        SettingsKey = settingsKey;
        Settings = settings;
    }

    /// <summary>
    ///     Gets or sets the content key.
    /// </summary>
    /// <value>
    ///     The content key.
    /// </value>
    public Guid ContentKey { get; set; }

    /// <summary>
    ///     Gets or sets the settings key.
    /// </summary>
    /// <value>
    ///     The settings key.
    /// </value>
    public Guid? SettingsKey { get; set; }

    /// <summary>
    ///     Gets the content.
    /// </summary>
    /// <value>
    ///     The content.
    /// </value>
    public IPublishedElement Content { get; }

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
    /// <param name="contentKey">The content key.</param>
    /// <param name="content">The content.</param>
    /// <param name="settingsKey">The settings key.</param>
    /// <param name="settings">The settings.</param>
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
    /// <param name="contentKey">The content key.</param>
    /// <param name="content">The content.</param>
    /// <param name="settingsKey">The settings key.</param>
    /// <param name="settings">The settings.</param>
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
