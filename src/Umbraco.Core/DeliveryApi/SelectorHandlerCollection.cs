using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

public class SelectorHandlerCollection : BuilderCollectionBase<ISelectorHandler>
{
    public SelectorHandlerCollection(Func<IEnumerable<ISelectorHandler>> items)
        : base(items)
    {
    }
}
