using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class FilterHandlerCollection : BuilderCollectionBase<IFilterHandler>
{
    public FilterHandlerCollection(Func<IEnumerable<IFilterHandler>> items)
        : base(items)
    {
    }
}
