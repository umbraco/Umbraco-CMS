using Umbraco.Core.Composing;

namespace Umbraco.Web.Routing
{
    public class ContentFinderCollectionBuilder : OrderedCollectionBuilderBase<ContentFinderCollectionBuilder, ContentFinderCollection, IContentFinder>
    {
        protected override ContentFinderCollectionBuilder This => this;
    }
}
