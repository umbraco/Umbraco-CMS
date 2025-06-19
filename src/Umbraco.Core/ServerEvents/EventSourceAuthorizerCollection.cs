using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ServerEvents;

public class EventSourceAuthorizerCollection : BuilderCollectionBase<IEventSourceAuthorizer>
{
    public EventSourceAuthorizerCollection(Func<IEnumerable<IEventSourceAuthorizer>> items)
        : base(items)
    {
    }
}
