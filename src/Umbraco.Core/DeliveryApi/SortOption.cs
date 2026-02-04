namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Represents a sort option used for ordering query results in the Delivery API.
/// </summary>
public sealed class SortOption
{
    /// <summary>
    ///     Gets or sets the name of the field to sort by.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    ///     Gets or sets the sort direction.
    /// </summary>
    public required Direction Direction { get; set; }
}
