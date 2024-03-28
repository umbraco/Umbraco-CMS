// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// The strongly typed model for the Block List editor.
/// </summary>
/// <seealso cref="ReadOnlyCollection{BlockGridItem}" />
[DataContract(Name = "blockgrid", Namespace = "")]
public class BlockGridModel : BlockModelCollection<BlockGridItem>
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
        : this(new List<BlockGridItem>(), null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockGridModel" /> class.
    /// </summary>
    /// <param name="list">The list to wrap.</param>
    /// <param name="gridColumns">The number of columns in the grid.</param>
    public BlockGridModel(IList<BlockGridItem> list, int? gridColumns)
        : base(list) => GridColumns = gridColumns;

    /// <summary>
    /// The number of columns in the grid.
    /// </summary>
    public int? GridColumns { get; }
}
