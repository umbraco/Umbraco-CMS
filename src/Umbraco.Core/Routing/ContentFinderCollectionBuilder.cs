using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

public class ContentFinderCollectionBuilder : OrderedCollectionBuilderBase<ContentFinderCollectionBuilder, ContentFinderCollection, IContentFinder>
{
    protected override ContentFinderCollectionBuilder This => this;
}
