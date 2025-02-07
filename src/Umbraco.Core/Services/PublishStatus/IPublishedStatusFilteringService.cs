using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Services.Navigation;

public interface IPublishedStatusFilteringService
{
    IEnumerable<IPublishedContent> FilterAncestors(IEnumerable<Guid> ancestorsKeys, string? culture);

    IEnumerable<IPublishedContent> FilterSiblings(IEnumerable<Guid> siblingKeys, string? culture);

    IEnumerable<IPublishedContent> FilterChildren(IEnumerable<Guid> childrenKeys, string? culture);

    IEnumerable<IPublishedContent> FilterDescendants(IEnumerable<Guid> descendantKeys, string? culture);
}
