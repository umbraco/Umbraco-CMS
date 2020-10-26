using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;

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
        IFactory Configure(IServiceCollection services);

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
