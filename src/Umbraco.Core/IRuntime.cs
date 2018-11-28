using Umbraco.Core.Composing;

namespace Umbraco.Core
{
    /// <summary>
    /// Defines the Umbraco runtime.
    /// </summary>
    public interface IRuntime
    {
        /// <summary>
        /// Boots the runtime.
        /// </summary>
        /// <param name="register">The application register.</param>
        void Boot(IRegister register);

        /// <summary>
        /// Gets the runtime state.
        /// </summary>
        IRuntimeState State { get; }

        /// <summary>
        /// Terminates the runtime.
        /// </summary>
        void Terminate();
    }
}
