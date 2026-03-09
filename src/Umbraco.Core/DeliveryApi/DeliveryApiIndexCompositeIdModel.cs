namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Represents the decomposed components of a composite ID used in the Delivery API index.
/// </summary>
public class DeliveryApiIndexCompositeIdModel
{
    /// <summary>
    ///     Gets or sets the content ID component of the composite ID.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    ///     Gets or sets the culture component of the composite ID.
    /// </summary>
    public string? Culture { get; set; }
}
