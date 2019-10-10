namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Base class for user components that only requires implmenting the <see cref="Initialize" /> method.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Composing.IUserComponent" />
    public abstract class UserComponentBase : IUserComponent
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
