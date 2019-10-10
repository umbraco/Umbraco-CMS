using Umbraco.Core.Composing;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents a user component.
    /// </summary>
    /// <remarks>
    /// User components are automatically discovered and composed.
    /// </remarks>
    /// <seealso cref="Umbraco.Core.Composing.IComponent" />
    /// <seealso cref="Umbraco.Core.Composing.IDiscoverable" />
    public interface IUserComponent : IComponent, IDiscoverable
    { }
}
