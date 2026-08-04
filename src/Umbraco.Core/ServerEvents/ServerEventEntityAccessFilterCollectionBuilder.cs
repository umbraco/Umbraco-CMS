using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Builds the <see cref="ServerEventEntityAccessFilterCollection"/> by allowing registration of
/// <see cref="IServerEventEntityAccessFilter"/> instances.
/// </summary>
/// <remarks>
/// Use this builder to register custom entity access filters that gate which connected users receive
/// entity-scoped server events. The filters are executed in order.
/// </remarks>
public class ServerEventEntityAccessFilterCollectionBuilder : OrderedCollectionBuilderBase<ServerEventEntityAccessFilterCollectionBuilder, ServerEventEntityAccessFilterCollection, IServerEventEntityAccessFilter>
{
    /// <inheritdoc />
    protected override ServerEventEntityAccessFilterCollectionBuilder This => this;
}
