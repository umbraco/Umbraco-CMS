using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Routing
{
    public class ContentFinderCollectionBuilder : OrderedCollectionBuilderBase<ContentFinderCollectionBuilder, ContentFinderCollection, IContentFinder>
    {
        public ContentFinderCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override ContentFinderCollectionBuilder This => this;
    }
}
