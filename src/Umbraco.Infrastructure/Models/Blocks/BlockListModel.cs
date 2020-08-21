using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// The strongly typed model for the Block List editor
    /// </summary>
    [DataContract(Name = "blockList", Namespace = "")]
    public class BlockListModel : IReadOnlyList<BlockListItem>
    {
        private readonly IReadOnlyList<BlockListItem> _layout = new List<BlockListItem>();

        public static BlockListModel Empty { get; } = new BlockListModel();

        private BlockListModel()
        {
        }

        public BlockListModel(IEnumerable<BlockListItem> layout)
        {
            _layout = layout.ToList();
        }

        public int Count => _layout.Count;

        /// <summary>
        /// Get the block by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public BlockListItem this[int index] => _layout[index];

        /// <summary>
        /// Get the block by content Guid
        /// </summary>
        /// <param name="contentKey"></param>
        /// <returns></returns>
        public BlockListItem this[Guid contentKey] => _layout.FirstOrDefault(x => x.Content.Key == contentKey);

        /// <summary>
        /// Get the block by content element Udi
        /// </summary>
        /// <param name="contentUdi"></param>
        /// <returns></returns>
        public BlockListItem this[Udi contentUdi]
        {
            get
            {
                if (!(contentUdi is GuidUdi guidUdi)) return null;
                return _layout.FirstOrDefault(x => x.Content.Key == guidUdi.Guid);
            }
        }

        public IEnumerator<BlockListItem> GetEnumerator() => _layout.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
