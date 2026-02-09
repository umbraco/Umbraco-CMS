namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a block list model in the Delivery API.
/// </summary>
public sealed class ApiBlockListModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiBlockListModel" /> class.
    /// </summary>
    /// <param name="items">The block items in the list.</param>
    public ApiBlockListModel(IEnumerable<ApiBlockItem> items) => Items = items;

    /// <summary>
    ///     Gets the block items in the list.
    /// </summary>
    public IEnumerable<ApiBlockItem> Items { get; }
}
