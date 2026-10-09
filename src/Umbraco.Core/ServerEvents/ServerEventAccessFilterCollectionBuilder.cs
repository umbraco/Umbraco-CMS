using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Builds the <see cref="ServerEventAccessFilterCollection"/> by allowing registration of
/// <see cref="IServerEventAccessFilter"/> instances.
/// </summary>
/// <remarks>
/// Use this builder to register custom access filters that gate which connected users receive
/// server events for a given source. The filters are executed in order.
/// </remarks>
public class ServerEventAccessFilterCollectionBuilder : OrderedCollectionBuilderBase<ServerEventAccessFilterCollectionBuilder, ServerEventAccessFilterCollection, IServerEventAccessFilter>
{
    /// <inheritdoc />
    protected override ServerEventAccessFilterCollectionBuilder This => this;
}
