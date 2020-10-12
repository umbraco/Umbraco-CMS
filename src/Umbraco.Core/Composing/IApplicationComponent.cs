namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents an application component.
    /// </summary>
    /// <remarks>
    /// Application components are automatically discovered and composed.
    /// </remarks>
    /// <seealso cref="Umbraco.Core.Composing.IComponent" />
    /// <seealso cref="Umbraco.Core.Composing.IDiscoverable" />
    public interface IApplicationComponent : IComponent, IDiscoverable
    { }
}
