namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Represents a selector option used for content querying in the Delivery API.
/// </summary>
public sealed class SelectorOption
{
    /// <summary>
    ///     Gets or sets the name of the field to select on.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    ///     Gets or sets the values to match against the field.
    /// </summary>
    public required string[] Values { get; set; }
}
