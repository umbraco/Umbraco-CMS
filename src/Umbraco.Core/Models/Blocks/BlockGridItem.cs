using System;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models.Blocks
{
    /// <summary>
    /// Represents a layout item for the Block Grid editor.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Models.Blocks.IBlockReference{Umbraco.Core.Models.PublishedContent.IPublishedElement}" />

    // TODO: Change: Niels: this is also called block, be aware!
    [DataContract(Name = "block", Namespace = "")]
    public class BlockGridItem : IBlockReference<IPublishedElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGridItem" /> class.
        /// </summary>
        /// <param name="contentUdi">The content UDI.</param>
        /// <param name="content">The content.</param>
        /// <param name="settingsUdi">The settings UDI.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="rowSpan">The number of rows to span</param>
        /// <param name="columnSpan">The number of columns to span</param>
        /// <exception cref="System.ArgumentNullException">contentUdi
        /// or
        /// content</exception>
        public BlockGridItem(Udi contentUdi, IPublishedElement content, Udi settingsUdi, IPublishedElement settings, int rowSpan, int columnSpan)
        {
            ContentUdi = contentUdi ?? throw new ArgumentNullException(nameof(contentUdi));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            SettingsUdi = settingsUdi;
            Settings = settings;
            RowSpan = rowSpan;
            ColumnSpan = columnSpan;
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
        public int RowSpan { get; }

        /// <summary>
        /// The number of columns this item should span
        /// </summary>
        [DataMember(Name = "columnSpan")]
        public int ColumnSpan { get; }

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
    }

    /// <summary>
    /// Represents a layout item with a generic content type for the Block List editor.
    /// </summary>
    /// <typeparam name="T">The type of the content.</typeparam>
    /// <seealso cref="Umbraco.Core.Models.Blocks.IBlockReference{Umbraco.Core.Models.PublishedContent.IPublishedElement}" />
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
        /// <param name="rowSpan">The number of rows to span</param>
        /// <param name="columnSpan">The number of columns to span</param>
        public BlockGridItem(Udi contentUdi, T content, Udi settingsUdi, IPublishedElement settings, int rowSpan, int columnSpan)
            : base(contentUdi, content, settingsUdi, settings, rowSpan, columnSpan)
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
    /// <seealso cref="Umbraco.Core.Models.Blocks.IBlockReference{Umbraco.Core.Models.PublishedContent.IPublishedElement}" />
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
        /// <param name="rowSpan">The number of rows to span</param>
        /// <param name="columnSpan">The number of columns to span</param>
        public BlockGridItem(Udi contentUdi, TContent content, Udi settingsUdi, TSettings settings, int rowSpan, int columnSpan)
            : base(contentUdi, content, settingsUdi, settings, rowSpan, columnSpan)
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
