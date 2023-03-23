using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public interface IRequestStartItemService
{
    /// <summary>
    ///     Gets the requested start item from the "Start-Item" header, if present.
    /// </summary>
    IPublishedContent? GetStartItem();
}
