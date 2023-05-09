using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

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
