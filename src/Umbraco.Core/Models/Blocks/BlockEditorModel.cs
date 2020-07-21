using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// The base class for any strongly typed model for a Block editor implementation
    /// </summary>
    public abstract class BlockEditorModel
    {
        protected BlockEditorModel(IEnumerable<IPublishedElement> contentData, IEnumerable<IPublishedElement> settingsData)
        {
            ContentData = contentData ?? throw new ArgumentNullException(nameof(contentData));
            SettingsData = settingsData ?? new List<IPublishedContent>();
        }

        public BlockEditorModel()
        {
        }


        /// <summary>
        /// The content data items of the Block List editor
        /// </summary>
        [DataMember(Name = "contentData")]
        public IEnumerable<IPublishedElement> ContentData { get; set; } = new List<IPublishedContent>();

        /// <summary>
        /// The settings data items of the Block List editor
        /// </summary>
        [DataMember(Name = "settingsData")]
        public IEnumerable<IPublishedElement> SettingsData { get; set; } = new List<IPublishedContent>();
    }
}
