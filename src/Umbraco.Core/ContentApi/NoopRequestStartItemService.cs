using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class NoopRequestStartItemService : IRequestStartItemService
{
    public IPublishedContent? GetStartItem() => null;
}
