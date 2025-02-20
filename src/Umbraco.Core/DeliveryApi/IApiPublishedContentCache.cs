using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiPublishedContentCache
{
    IPublishedContent? GetByRoute(string route);

    IPublishedContent? GetById(Guid contentId);

    IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> contentIds);

    Task<IPublishedContent?> GetByIdAsync(Guid contentId) => Task.FromResult(GetById(contentId));
    Task<IPublishedContent?> GetByRouteAsync(string route) => Task.FromResult(GetByRoute(route));
    Task<IEnumerable<IPublishedContent>> GetByIdsAsync(IEnumerable<Guid> contentIds) => Task.FromResult(GetByIds(contentIds));
}
