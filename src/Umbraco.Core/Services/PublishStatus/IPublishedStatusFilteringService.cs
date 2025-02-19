using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Services.Navigation;

public interface IPublishedStatusFilteringService
{
    IEnumerable<IPublishedContent> FilterAvailable(IEnumerable<Guid> candidateKeys, string? culture);
}
