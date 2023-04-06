using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public class SortHandlerCollection : BuilderCollectionBase<ISortHandler>
{
    public SortHandlerCollection(Func<IEnumerable<ISortHandler>> items)
        : base(items)
    {
    }
}
