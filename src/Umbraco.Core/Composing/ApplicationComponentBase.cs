namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Base class for application components that only requires implementing the <see cref="Initialize" /> method.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Composing.IApplicationComponent" />
    public abstract class ApplicationComponentBase : IApplicationComponent
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
