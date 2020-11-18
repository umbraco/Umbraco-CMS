using System;

namespace Umbraco.Core
{
    /// <summary>
    /// Defines the Umbraco runtime.
    /// </summary>
    public interface IRuntime
    {
        /// <summary>
        /// Gets the runtime state.
        /// </summary>
        IRuntimeState State { get; }

        void Start();

        /// <summary>
        /// Terminates the runtime.
        /// </summary>
        void Terminate();
    }
}
