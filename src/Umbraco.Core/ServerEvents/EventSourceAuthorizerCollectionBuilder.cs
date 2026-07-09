using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Builds the <see cref="EventSourceAuthorizerCollection"/> by allowing registration of <see cref="IEventSourceAuthorizer"/> instances.
/// </summary>
/// <remarks>
/// Use this builder to register custom event source authorizers that control access to server-sent event sources.
/// The authorizers are executed in order, allowing for priority-based authorization logic.
/// </remarks>
public class EventSourceAuthorizerCollectionBuilder : OrderedCollectionBuilderBase<EventSourceAuthorizerCollectionBuilder, EventSourceAuthorizerCollection, IEventSourceAuthorizer>
{
    /// <inheritdoc />
    protected override EventSourceAuthorizerCollectionBuilder This => this;
}
