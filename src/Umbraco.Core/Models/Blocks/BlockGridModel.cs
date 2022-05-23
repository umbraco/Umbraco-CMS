using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks
{
    /// <summary>
    /// The strongly typed model for the Block List editor.
    /// </summary>
    /// <seealso cref="ReadOnlyCollection{BlockGridItem}" />
    [DataContract(Name = "blockgrid", Namespace = "")]
    public class BlockGridModel : ReadOnlyCollection<BlockGridItem>
    {
        /// <summary>
        /// Gets the empty <see cref="BlockGridModel" />.
        /// </summary>
        /// <value>
        /// The empty <see cref="BlockGridModel" />.
        /// </value>
        public static BlockGridModel Empty { get; } = new BlockGridModel();

        /// <summary>
        /// Prevents a default instance of the <see cref="BlockGridModel" /> class from being created.
        /// </summary>
        private BlockGridModel()
            : this(new List<BlockGridItem>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGridModel" /> class.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        public BlockGridModel(IList<BlockGridItem> list)
            : base(list)
        { }

        /// <summary>
        /// Gets the <see cref="BlockGridItem" /> with the specified content key.
        /// </summary>
        /// <value>
        /// The <see cref="BlockGridItem" />.
        /// </value>
        /// <param name="contentKey">The content key.</param>
        /// <returns>
        /// The <see cref="BlockGridItem" /> with the specified content key.
        /// </returns>
        public BlockGridItem? this[Guid contentKey] => this.FirstOrDefault(x => x.Content.Key == contentKey);

        /// <summary>
        /// Gets the <see cref="BlockGridItem" /> with the specified content UDI.
        /// </summary>
        /// <value>
        /// The <see cref="BlockGridItem" />.
        /// </value>
        /// <param name="contentUdi">The content UDI.</param>
        /// <returns>
        /// The <see cref="BlockGridItem" /> with the specified content UDI.
        /// </returns>
        public BlockGridItem? this[Udi contentUdi] => contentUdi is GuidUdi guidUdi ? this.FirstOrDefault(x => x.Content.Key == guidUdi.Guid) : null;
    }
}
