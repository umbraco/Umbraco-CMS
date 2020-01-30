using System.Collections.Generic;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// The strongly typed model for the Block List editor
    /// </summary>
    public class BlockListModel : BlockEditorModel
    {
        /// <summary>
        /// The layout items of the Block List editor
        /// </summary>
        public IEnumerable<BlockListLayoutReference> Layout { get; }

        
    }
}
