using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Core.Services
{
    /// <summary>
    /// Defines the Umbraco runtime.
    /// </summary>
    public interface IRuntime : IHostedService
    {
        /// <summary>
        /// Gets the runtime state.
        /// </summary>
        IRuntimeState State { get; }
    }
}
