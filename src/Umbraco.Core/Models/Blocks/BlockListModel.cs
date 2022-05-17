using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     The strongly typed model for the Block List editor.
/// </summary>
/// <seealso cref="ReadOnlyCollection{BlockListItem}" />
[DataContract(Name = "blockList", Namespace = "")]
public class BlockListModel : ReadOnlyCollection<BlockListItem>
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

    /// <summary>
    ///     Gets the <see cref="BlockListItem" /> with the specified content key.
    /// </summary>
    /// <value>
    ///     The <see cref="BlockListItem" />.
    /// </value>
    /// <param name="contentKey">The content key.</param>
    /// <returns>
    ///     The <see cref="BlockListItem" /> with the specified content key.
    /// </returns>
    public BlockListItem? this[Guid contentKey] => this.FirstOrDefault(x => x.Content.Key == contentKey);

    /// <summary>
    ///     Gets the <see cref="BlockListItem" /> with the specified content UDI.
    /// </summary>
    /// <value>
    ///     The <see cref="BlockListItem" />.
    /// </value>
    /// <param name="contentUdi">The content UDI.</param>
    /// <returns>
    ///     The <see cref="BlockListItem" /> with the specified content UDI.
    /// </returns>
    public BlockListItem? this[Udi contentUdi] => contentUdi is GuidUdi guidUdi
        ? this.FirstOrDefault(x => x.Content.Key == guidUdi.Guid)
        : null;
}
