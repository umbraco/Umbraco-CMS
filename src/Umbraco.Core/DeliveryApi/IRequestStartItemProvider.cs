using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a provider that retrieves the start item for Delivery API requests.
/// </summary>
public interface IRequestStartItemProvider
{
    /// <summary>
    ///     Gets the requested start item, if present.
    /// </summary>
    IPublishedContent? GetStartItem();

    /// <summary>
    ///     Gets the value of the requested start item, if present.
    /// </summary>
    string? RequestedStartItem();
}
