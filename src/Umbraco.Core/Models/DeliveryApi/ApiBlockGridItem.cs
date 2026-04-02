namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a block grid item in the Delivery API.
/// </summary>
public sealed class ApiBlockGridItem : ApiBlockItem
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiBlockGridItem" /> class.
    /// </summary>
    /// <param name="content">The content element of the block.</param>
    /// <param name="settings">The optional settings element of the block.</param>
    /// <param name="rowSpan">The number of rows the block spans.</param>
    /// <param name="columnSpan">The number of columns the block spans.</param>
    /// <param name="areaGridColumns">The number of columns in the block's area grid.</param>
    /// <param name="areas">The areas within the block.</param>
    public ApiBlockGridItem(IApiElement content, IApiElement? settings, int rowSpan, int columnSpan, int areaGridColumns, IEnumerable<ApiBlockGridArea> areas)
        : base(content, settings)
    {
        RowSpan = rowSpan;
        ColumnSpan = columnSpan;
        AreaGridColumns = areaGridColumns;
        Areas = areas;
    }

    /// <summary>
    ///     Gets the number of rows the block spans.
    /// </summary>
    public int RowSpan { get; }

    /// <summary>
    ///     Gets the number of columns the block spans.
    /// </summary>
    public int ColumnSpan { get; }

    /// <summary>
    ///     Gets the number of columns in the block's area grid.
    /// </summary>
    public int AreaGridColumns { get; }

    /// <summary>
    ///     Gets the areas within the block.
    /// </summary>
    public IEnumerable<ApiBlockGridArea> Areas { get; }
}
