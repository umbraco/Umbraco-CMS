using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public interface IOutputExpansionStrategy
{
    bool ShouldExpand(IPublishedPropertyType propertyType);
}
