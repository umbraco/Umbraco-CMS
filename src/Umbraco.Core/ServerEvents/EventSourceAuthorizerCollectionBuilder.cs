using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ServerEvents;

public class EventSourceAuthorizerCollectionBuilder : OrderedCollectionBuilderBase<EventSourceAuthorizerCollectionBuilder, EventSourceAuthorizerCollection, IEventSourceAuthorizer>
{
    protected override EventSourceAuthorizerCollectionBuilder This => this;
}
