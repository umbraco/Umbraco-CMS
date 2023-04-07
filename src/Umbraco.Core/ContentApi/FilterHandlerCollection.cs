using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public class FilterHandlerCollection : BuilderCollectionBase<IFilterHandler>
{
    public FilterHandlerCollection(Func<IEnumerable<IFilterHandler>> items)
        : base(items)
    {
    }
}
