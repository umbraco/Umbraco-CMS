using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Trees
{
    public class SearchableTreeCollectionBuilder : LazyCollectionBuilderBase<SearchableTreeCollectionBuilder, SearchableTreeCollection, ISearchableTree>
    {
        protected override SearchableTreeCollectionBuilder This => this;

        //per request because generally an instance of ISearchableTree is a controller
        protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
    }
}
