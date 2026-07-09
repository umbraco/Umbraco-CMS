namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Represents a field value for the Delivery API content index.
/// </summary>
public sealed class IndexFieldValue
{
    /// <summary>
    ///     Gets or sets the name of the index field.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    ///     Gets or sets the values for this index field.
    /// </summary>
    public required IEnumerable<object> Values { get; set; }
}
