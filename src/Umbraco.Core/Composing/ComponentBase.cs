namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Base class for components that only requires implementing the <see cref="Initialize" /> method.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Composing.IComponent" />
    public abstract class ComponentBase : IComponent
    {
        /// <summary>
        /// Initializes this component.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Terminates this component.
        /// </summary>
        public virtual void Terminate()
        {
            // Nothing to terminate
        }
    }
}
