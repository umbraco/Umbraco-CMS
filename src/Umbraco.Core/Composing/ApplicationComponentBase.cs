namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Base class for application components that are automatically composed and only requires implementing the <see cref="Initialize" /> method.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Composing.ComponentBase" />
    /// <seealso cref="Umbraco.Core.Composing.IApplicationComponent" />
    public abstract class ApplicationComponentBase : ComponentBase, IApplicationComponent
    { }
}
