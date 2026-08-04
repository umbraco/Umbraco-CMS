using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Represents a collection of <see cref="IServerEventEntityAccessFilter"/> instances.
/// </summary>
/// <remarks>
/// This collection contains all registered entity access filters that determine which connected
/// users may receive an entity-scoped server event (for example document or media).
/// </remarks>
public class ServerEventEntityAccessFilterCollection : BuilderCollectionBase<IServerEventEntityAccessFilter>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventEntityAccessFilterCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that provides the entity access filters.</param>
    public ServerEventEntityAccessFilterCollection(Func<IEnumerable<IServerEventEntityAccessFilter>> items)
        : base(items)
    {
    }
}
