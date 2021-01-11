namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents an application component that is automatically composed.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Composing.IComponent" />
    /// <seealso cref="Umbraco.Core.Composing.IDiscoverable" />
    public interface IApplicationComponent : IComponent, IDiscoverable
    { }
}
