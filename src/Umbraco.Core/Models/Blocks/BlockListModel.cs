using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// The strongly typed model for the Block List editor
    /// </summary>
    [DataContract(Name = "blockList", Namespace = "")]
    public class BlockListModel : BlockEditorModel
    {
        public static BlockListModel Empty { get; } = new BlockListModel();

        private BlockListModel()
        {
        }

        public BlockListModel(IEnumerable<IPublishedElement> contentData, IEnumerable<IPublishedElement> settingsData, IEnumerable<BlockListLayoutReference> layout)
            : base(contentData, settingsData)
        {
            Layout = layout;
        }

        /// <summary>
        /// The layout items of the Block List editor
        /// </summary>
        [DataMember(Name = "layout")]
        public IEnumerable<BlockListLayoutReference> Layout { get; } = new List<BlockListLayoutReference>();

        
    }
}
