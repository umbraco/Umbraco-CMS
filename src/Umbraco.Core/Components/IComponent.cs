namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents a component.
    /// </summary>
    /// <remarks>
    /// <para>Components are created by DI and therefore must have a public constructor.</para>
    /// <para>All components which are also disposable, will be disposed in reverse
    /// order, when Umbraco terminates.</para>
    /// <para>The Dispose method may be invoked more than once, and components
    /// should ensure they support this.</para>
    /// </remarks>
    public interface IComponent
    { }
}
