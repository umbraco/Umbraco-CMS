namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Represents a filter option used for filtering query results in the Delivery API.
/// </summary>
public sealed class FilterOption
{
    /// <summary>
    ///     Gets or sets the name of the field to filter on.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    ///     Gets or sets the values to filter against.
    /// </summary>
    public required string[] Values { get; set; }

    /// <summary>
    ///     Gets or sets the filter operation to apply.
    /// </summary>
    public required FilterOperation Operator { get; set; }
}
