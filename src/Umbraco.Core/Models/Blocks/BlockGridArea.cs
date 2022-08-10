using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

[DataContract(Name = "area", Namespace = "")]
public class BlockGridArea : ReadOnlyCollection<BlockGridItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockGridArea" /> class.
    /// </summary>
    /// <param name="list">The list to wrap.</param>
    /// <param name="alias">The area alias</param>
    public BlockGridArea(IList<BlockGridItem> list, string alias) : base(list) => Alias = alias;

    /// <summary>
    /// The area alias
    /// </summary>
    [DataMember(Name = "alias")]
    public string Alias { get; }

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
