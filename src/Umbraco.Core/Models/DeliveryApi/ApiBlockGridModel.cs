namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a block grid model in the Delivery API.
/// </summary>
public sealed class ApiBlockGridModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiBlockGridModel" /> class.
    /// </summary>
    /// <param name="gridColumns">The number of columns in the grid.</param>
    /// <param name="items">The block grid items in the model.</param>
    public ApiBlockGridModel(int gridColumns, IEnumerable<ApiBlockGridItem> items)
    {
        GridColumns = gridColumns;
        Items = items;
    }

    /// <summary>
    ///     Gets the number of columns in the grid.
    /// </summary>
    public int GridColumns { get; }

    /// <summary>
    ///     Gets the block grid items in the model.
    /// </summary>
    public IEnumerable<ApiBlockGridItem> Items { get; }
}
