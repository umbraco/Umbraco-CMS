using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class DefaultOutputExpansionStrategy : IOutputExpansionStrategy
{
    // TODO: implement this (will probably need to move to another project to access the current HTTP context)
    public bool ShouldExpand(IPublishedPropertyType propertyType) => false;
}
