using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// The base class for any strongly typed model for a Block editor implementation
    /// </summary>
    public abstract class BlockEditorModel
    {
        /// <summary>
        /// The data items of the Block List editor
        /// </summary>
        public IEnumerable<IPublishedElement> Data { get; }
    }
}
