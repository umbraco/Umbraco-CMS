using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Navigable;

internal interface INavigableData
{
    IPublishedContent? GetById(bool preview, int contentId);

    IEnumerable<IPublishedContent> GetAtRoot(bool preview);
}
