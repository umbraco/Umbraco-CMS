using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public class
    QueryHandlerCollectionBuilder : LazyCollectionBuilderBase<QueryHandlerCollectionBuilder, QueryHandlerCollection,
        IQueryHandler>
{
    protected override QueryHandlerCollectionBuilder This => this;
}
