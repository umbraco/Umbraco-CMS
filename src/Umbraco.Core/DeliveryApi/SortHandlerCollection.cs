using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

public class SortHandlerCollection : BuilderCollectionBase<ISortHandler>
{
    public SortHandlerCollection(Func<IEnumerable<ISortHandler>> items)
        : base(items)
    {
    }
}
