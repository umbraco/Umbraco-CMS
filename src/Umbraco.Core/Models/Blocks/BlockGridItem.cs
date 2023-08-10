// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models.Blocks
{
    /// <summary>
    /// Represents a layout item for the Block Grid editor.
    /// </summary>
    /// <seealso cref="IBlockReference{TContent,TSettings}" />
    [DataContract(Name = "block", Namespace = "")]
    public class BlockGridItem : IBlockReference<IPublishedElement, IPublishedElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGridItem" /> class.
        /// </summary>
        /// <param name="contentUdi">The content UDI.</param>
        /// <param name="content">The content.</param>
        /// <param name="settingsUdi">The settings UDI.</param>
        /// <param name="settings">The settings.</param>
        /// <exception cref="ArgumentNullException">contentUdi
        /// or
        /// content</exception>
        public BlockGridItem(Udi contentUdi, IPublishedElement content, Udi settingsUdi, IPublishedElement settings)
        {
            ContentUdi = contentUdi ?? throw new ArgumentNullException(nameof(contentUdi));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            SettingsUdi = settingsUdi;
            Settings = settings;
        }

        /// <summary>
        /// Gets the content UDI.
        /// </summary>
        /// <value>
        /// The content UDI.
        /// </value>
        [DataMember(Name = "contentUdi")]
        public Udi ContentUdi { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [DataMember(Name = "content")]
        public IPublishedElement Content { get; }

        /// <summary>
        /// Gets the settings UDI.
        /// </summary>
        /// <value>
        /// The settings UDI.
        /// </value>
        [DataMember(Name = "settingsUdi")]
        public Udi SettingsUdi { get; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        [DataMember(Name = "settings")]
        public IPublishedElement Settings { get; }

        /// <summary>
        /// The number of rows this item should span
        /// </summary>
        [DataMember(Name = "rowSpan")]
        public int RowSpan { get; set; }

        /// <summary>
        /// The number of columns this item should span
        /// </summary>
        [DataMember(Name = "columnSpan")]
        public int ColumnSpan { get; set; }

        /// <summary>
        /// The grid areas within this item
        /// </summary>
        [DataMember(Name = "areas")]
        public IEnumerable<BlockGridArea> Areas { get; set; } = Array.Empty<BlockGridArea>();

        /// <summary>
        /// The number of columns available for the areas to span
        /// </summary>
        [DataMember(Name = "areaGridColumns")]
        public int? AreaGridColumns { get; set; }

        /// <summary>
        /// The number of columns in the root grid
        /// </summary>
        [DataMember(Name = "gridColumns")]
        public int? GridColumns { get; set; }
    }

    /// <summary>
    /// Represents a layout item with a generic content type for the Block List editor.
    /// </summary>
    /// <typeparam name="T">The type of the content.</typeparam>
    public class BlockGridItem<T> : BlockGridItem
        where T : IPublishedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGridItem{T}" /> class.
        /// </summary>
        /// <param name="contentUdi">The content UDI.</param>
        /// <param name="content">The content.</param>
        /// <param name="settingsUdi">The settings UDI.</param>
        /// <param name="settings">The settings.</param>
        public BlockGridItem(Udi contentUdi, T content, Udi settingsUdi, IPublishedElement settings)
            : base(contentUdi, content, settingsUdi, settings)
        {
            Content = content;
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public new T Content { get; }
    }

    /// <summary>
    /// Represents a layout item with generic content and settings types for the Block List editor.
    /// </summary>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    public class BlockGridItem<TContent, TSettings> : BlockGridItem<TContent>
        where TContent : IPublishedElement
        where TSettings : IPublishedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGridItem{TContent, TSettings}" /> class.
        /// </summary>
        /// <param name="contentUdi">The content udi.</param>
        /// <param name="content">The content.</param>
        /// <param name="settingsUdi">The settings udi.</param>
        /// <param name="settings">The settings.</param>
        public BlockGridItem(Udi contentUdi, TContent content, Udi settingsUdi, TSettings settings)
            : base(contentUdi, content, settingsUdi, settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public new TSettings Settings { get; }
    }
}
