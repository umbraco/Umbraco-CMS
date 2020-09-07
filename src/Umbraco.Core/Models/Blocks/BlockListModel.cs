using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// The strongly typed model for the Block List editor.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IReadOnlyList{Umbraco.Core.Models.Blocks.BlockListItem}" />
    [DataContract(Name = "blockList", Namespace = "")]
    public class BlockListModel : IReadOnlyList<BlockListItem>
    {
        /// <summary>
        /// The layout.
        /// </summary>
        private readonly IReadOnlyList<BlockListItem> _layout;

        /// <summary>
        /// Gets the empty <see cref="BlockListModel" />.
        /// </summary>
        /// <value>
        /// The empty <see cref="BlockListModel" />.
        /// </value>
        public static BlockListModel Empty { get; } = new BlockListModel();

        /// <summary>
        /// Prevents a default instance of the <see cref="BlockListModel" /> class from being created.
        /// </summary>
        private BlockListModel()
            : this(new List<BlockListItem>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockListModel" /> class.
        /// </summary>
        /// <param name="layout">The layout.</param>
        /// <exception cref="System.ArgumentNullException">layout</exception>
        public BlockListModel(IEnumerable<BlockListItem> layout)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            _layout = layout.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count => _layout.Count;

        /// <summary>
        /// Gets the <see cref="BlockListItem" /> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="BlockListItem" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The <see cref="BlockListItem" /> at the specified index.
        /// </returns>
        public BlockListItem this[int index] => _layout[index];

        /// <summary>
        /// Gets the <see cref="BlockListItem" /> with the specified content key.
        /// </summary>
        /// <value>
        /// The <see cref="BlockListItem" />.
        /// </value>
        /// <param name="contentKey">The content key.</param>
        /// <returns>
        /// The <see cref="BlockListItem" /> with the specified content key.
        /// </returns>
        public BlockListItem this[Guid contentKey] => _layout.FirstOrDefault(x => x.Content.Key == contentKey);

        /// <summary>
        /// Gets the <see cref="BlockListItem" /> with the specified content UDI.
        /// </summary>
        /// <value>
        /// The <see cref="BlockListItem" />.
        /// </value>
        /// <param name="contentUdi">The content UDI.</param>
        /// <returns>
        /// The <see cref="BlockListItem" /> with the specified content UDI.
        /// </returns>
        public BlockListItem this[Udi contentUdi] => contentUdi is GuidUdi guidUdi ? _layout.FirstOrDefault(x => x.Content.Key == guidUdi.Guid) : null;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<BlockListItem> GetEnumerator() => _layout.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
