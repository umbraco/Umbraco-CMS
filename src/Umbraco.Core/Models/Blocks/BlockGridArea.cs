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
    /// <param name="rowSpan">The number of rows this area should span</param>
    /// <param name="columnSpan">The number of columns this area should span</param>
    public BlockGridArea(IList<BlockGridItem> list, string alias, int rowSpan, int columnSpan) : base(list)
    {
        Alias = alias;
        RowSpan = rowSpan;
        ColumnSpan = columnSpan;
    }

    /// <summary>
    /// The area alias
    /// </summary>
    [DataMember(Name = "alias")]
    public string Alias { get; }

    /// <summary>
    /// The number of rows this area should span.
    /// </summary>
    [DataMember(Name = "rowSpan")]
    public int RowSpan { get; }

    /// <summary>
    /// The number of columns this area should span.
    /// </summary>
    [DataMember(Name = "columnSpan")]
    public int ColumnSpan { get; }

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
