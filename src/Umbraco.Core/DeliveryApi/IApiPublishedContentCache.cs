using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiPublishedContentCache
{
    IPublishedContent? GetByRoute(string route);

    IPublishedContent? GetById(Guid contentId);

    IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> contentIds);
}
