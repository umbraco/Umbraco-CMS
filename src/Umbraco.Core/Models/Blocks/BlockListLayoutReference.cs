using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// Represents a layout item for the Block List editor
    /// </summary>
    [DataContract(Name = "blockListLayout", Namespace = "")]
    public class BlockListLayoutReference : IBlockReference<IPublishedElement>
    {
        public BlockListLayoutReference(Udi contentUdi, IPublishedElement content, Udi settingsUdi, IPublishedElement settings)
        {
            ContentUdi = contentUdi ?? throw new ArgumentNullException(nameof(contentUdi));            
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Settings = settings; // can be null
            SettingsUdi = settingsUdi; // can be null
        }

        /// <summary>
        /// The Id of the content data item
        /// </summary>
        [DataMember(Name = "contentUdi")]
        public Udi ContentUdi { get; }

        /// <summary>
        /// The Id of the settings data item
        /// </summary>
        [DataMember(Name = "settingsUdi")]
        public Udi SettingsUdi { get; }

        /// <summary>
        /// The content data item referenced
        /// </summary>
        /// <remarks>
        /// This is ignored from serialization since it is just a reference to the actual data element
        /// </remarks>
        [IgnoreDataMember]
        public IPublishedElement Content { get; }

        /// <summary>
        /// The settings data item referenced
        /// </summary>
        /// <remarks>
        /// This is ignored from serialization since it is just a reference to the actual data element
        /// </remarks>
        [IgnoreDataMember]
        public IPublishedElement Settings { get; }
    }
}
