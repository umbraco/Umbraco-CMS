using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Search
{
    internal class SearchableTreeCollectionBuilder : LazyCollectionBuilderBase<SearchableTreeCollectionBuilder, SearchableTreeCollection, ISearchableTree>
    {
        private readonly IApplicationTreeService _treeService;

        public SearchableTreeCollectionBuilder(IServiceContainer container, IApplicationTreeService treeService)
            : base(container)
        {
            _treeService = treeService;
        }

        protected override SearchableTreeCollectionBuilder This => this;

        public override SearchableTreeCollection CreateCollection()
        {
            return new SearchableTreeCollection(CreateItems(), _treeService);
        }

        //per request because generally an instance of ISearchableTree is a controller
        protected override ILifetime CollectionLifetime => new PerRequestLifeTime();
    }
}
