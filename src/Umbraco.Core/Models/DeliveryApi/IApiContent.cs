namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents content in the Delivery API.
/// </summary>
public interface IApiContent : IApiElement
{
    /// <summary>
    ///     Gets the name of the content.
    /// </summary>
    // TODO (V19): Remove this declaration; it is retained only to preserve the IApiContent.Name binary slot for
    // consumers compiled against v18.0. Name now lives on the IApiElement base interface.
    new string? Name { get; }

    /// <summary>
    ///     Gets the date and time when the content was created.
    /// </summary>
    public DateTime CreateDate { get; }

    /// <summary>
    ///     Gets the date and time when the content was last updated.
    /// </summary>
    public DateTime UpdateDate { get; }

    /// <summary>
    ///     Gets the route information for the content.
    /// </summary>
    IApiContentRoute Route { get; }
}
