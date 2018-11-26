using Umbraco.Core.Composing;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Search
{
    internal class SearchableTreeCollectionBuilder : LazyCollectionBuilderBase<SearchableTreeCollectionBuilder, SearchableTreeCollection, ISearchableTree>
    {
        protected override SearchableTreeCollectionBuilder This => this;
    }
}
