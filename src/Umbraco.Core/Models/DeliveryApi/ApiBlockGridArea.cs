namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents an area within a block grid item in the Delivery API.
/// </summary>
public sealed class ApiBlockGridArea
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiBlockGridArea" /> class.
    /// </summary>
    /// <param name="alias">The alias of the area.</param>
    /// <param name="rowSpan">The number of rows the area spans.</param>
    /// <param name="columnSpan">The number of columns the area spans.</param>
    /// <param name="items">The block grid items within the area.</param>
    public ApiBlockGridArea(string alias, int rowSpan, int columnSpan, IEnumerable<ApiBlockGridItem> items)
    {
        Alias = alias;
        RowSpan = rowSpan;
        ColumnSpan = columnSpan;
        Items = items;
    }

    /// <summary>
    ///     Gets the alias of the area.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    ///     Gets the number of rows the area spans.
    /// </summary>
    public int RowSpan { get; }

    /// <summary>
    ///     Gets the number of columns the area spans.
    /// </summary>
    public int ColumnSpan { get; }

    /// <summary>
    ///     Gets the block grid items within the area.
    /// </summary>
    public IEnumerable<ApiBlockGridItem> Items { get; }
}
