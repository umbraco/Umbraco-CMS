using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

[DataContract(Name = "area", Namespace = "")]
public class BlockGridArea : BlockModelCollection<BlockGridItem>
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
}
