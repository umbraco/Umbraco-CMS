using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Represents a collection of <see cref="IServerEventAccessFilter"/> instances.
/// </summary>
/// <remarks>
/// This collection contains all registered access filters that determine which connected users may
/// receive a server event for the sources they filter.
/// </remarks>
public class ServerEventAccessFilterCollection : BuilderCollectionBase<IServerEventAccessFilter>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventAccessFilterCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that provides the access filters.</param>
    public ServerEventAccessFilterCollection(Func<IEnumerable<IServerEventAccessFilter>> items)
        : base(items)
    {
    }
}
