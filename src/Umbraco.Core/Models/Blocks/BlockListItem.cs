using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a layout item for the Block List editor.
/// </summary>
/// <seealso cref="Umbraco.Core.Models.Blocks.IBlockReference{Umbraco.Core.Models.PublishedContent.IPublishedElement}" />
[DataContract(Name = "block", Namespace = "")]
public class BlockListItem : IBlockReference<IPublishedElement>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockListItem" /> class.
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
    public BlockListItem(Udi contentUdi, IPublishedElement content, Udi settingsUdi, IPublishedElement settings)
    {
        ContentUdi = contentUdi ?? throw new ArgumentNullException(nameof(contentUdi));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        SettingsUdi = settingsUdi;
        Settings = settings;
    }

    /// <summary>
    ///     Gets the content.
    /// </summary>
    /// <value>
    ///     The content.
    /// </value>
    [DataMember(Name = "content")]
    public IPublishedElement Content { get; }

    /// <summary>
    ///     Gets the settings UDI.
    /// </summary>
    /// <value>
    ///     The settings UDI.
    /// </value>
    [DataMember(Name = "settingsUdi")]
    public Udi SettingsUdi { get; }

    /// <summary>
    ///     Gets the content UDI.
    /// </summary>
    /// <value>
    ///     The content UDI.
    /// </value>
    [DataMember(Name = "contentUdi")]
    public Udi ContentUdi { get; }

    /// <summary>
    ///     Gets the settings.
    /// </summary>
    /// <value>
    ///     The settings.
    /// </value>
    [DataMember(Name = "settings")]
    public IPublishedElement Settings { get; }
}

/// <summary>
///     Represents a layout item with a generic content type for the Block List editor.
/// </summary>
/// <typeparam name="T">The type of the content.</typeparam>
/// <seealso cref="Umbraco.Core.Models.Blocks.IBlockReference{Umbraco.Core.Models.PublishedContent.IPublishedElement}" />
public class BlockListItem<T> : BlockListItem
    where T : IPublishedElement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockListItem{T}" /> class.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    /// <param name="content">The content.</param>
    /// <param name="settingsUdi">The settings UDI.</param>
    /// <param name="settings">The settings.</param>
    public BlockListItem(Udi contentUdi, T content, Udi settingsUdi, IPublishedElement settings)
        : base(contentUdi, content, settingsUdi, settings) =>
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
/// <seealso cref="Umbraco.Core.Models.Blocks.IBlockReference{Umbraco.Core.Models.PublishedContent.IPublishedElement}" />
public class BlockListItem<TContent, TSettings> : BlockListItem<TContent>
    where TContent : IPublishedElement
    where TSettings : IPublishedElement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockListItem{TContent, TSettings}" /> class.
    /// </summary>
    /// <param name="contentUdi">The content udi.</param>
    /// <param name="content">The content.</param>
    /// <param name="settingsUdi">The settings udi.</param>
    /// <param name="settings">The settings.</param>
    public BlockListItem(Udi contentUdi, TContent content, Udi settingsUdi, TSettings settings)
        : base(contentUdi, content, settingsUdi, settings) =>
        Settings = settings;

    /// <summary>
    ///     Gets the settings.
    /// </summary>
    /// <value>
    ///     The settings.
    /// </value>
    public new TSettings Settings { get; }
}
