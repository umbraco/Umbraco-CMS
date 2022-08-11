using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     The strongly typed model for the Block List editor.
/// </summary>
[DataContract(Name = "blockList", Namespace = "")]
public class BlockListModel : BlockModelCollection<BlockListItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockListModel" /> class.
    /// </summary>
    /// <param name="list">The list to wrap.</param>
    public BlockListModel(IList<BlockListItem> list)
        : base(list)
    {
    }

    /// <summary>
    ///     Prevents a default instance of the <see cref="BlockListModel" /> class from being created.
    /// </summary>
    private BlockListModel()
        : this(new List<BlockListItem>())
    {
    }

    /// <summary>
    ///     Gets the empty <see cref="BlockListModel" />.
    /// </summary>
    /// <value>
    ///     The empty <see cref="BlockListModel" />.
    /// </value>
    public static BlockListModel Empty { get; } = new();
}
