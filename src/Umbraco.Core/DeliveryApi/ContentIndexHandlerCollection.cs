using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ContentIndexHandlerCollection : BuilderCollectionBase<IContentIndexHandler>
{
    public ContentIndexHandlerCollection(Func<IEnumerable<IContentIndexHandler>> items)
        : base(items)
    {
    }
}
