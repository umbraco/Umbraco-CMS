using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestStartItemProvider
{
    /// <summary>
    ///     Gets the requested start item from the "Start-Item" header, if present.
    /// </summary>
    IPublishedContent? GetStartItem();

    /// <summary>
    ///     Checks whether the "Start-Item" header has value.
    /// </summary>
    bool StartItemHeaderHasValue();
}
