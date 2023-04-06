using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public class SelectorHandlerCollection : BuilderCollectionBase<ISelectorHandler>
{
    public SelectorHandlerCollection(Func<IEnumerable<ISelectorHandler>> items)
        : base(items)
    {
    }
}
