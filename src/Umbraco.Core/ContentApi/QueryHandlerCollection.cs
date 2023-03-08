using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public class QueryHandlerCollection : BuilderCollectionBase<IQueryHandler>
{
    public QueryHandlerCollection(Func<IEnumerable<IQueryHandler>> items)
        : base(items)
    {
    }
}
