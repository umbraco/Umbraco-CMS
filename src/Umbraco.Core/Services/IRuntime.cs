using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the Umbraco runtime.
/// </summary>
public interface IRuntime
{
    /// <summary>
    ///     Gets the runtime state.
    /// </summary>
    IRuntimeState State { get; }

    /// <summary>
    ///     Stops and Starts the runtime using the original cancellation token.
    /// </summary>
    Task RestartAsync();

    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
