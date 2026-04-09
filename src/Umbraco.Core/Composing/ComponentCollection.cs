using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
/// Represents the collection of <see cref="IAsyncComponent" /> implementations.
/// </summary>
public class ComponentCollection : BuilderCollectionBase<IAsyncComponent>
{
    private const int LogThresholdMilliseconds = 100;

    private readonly IProfilingLogger _profilingLogger;
    private readonly ILogger<ComponentCollection> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentCollection" /> class.
    /// </summary>
    /// <param name="items">A factory function that provides the component items.</param>
    /// <param name="profilingLogger">The profiling logger for timing component operations.</param>
    /// <param name="logger">The logger for recording component lifecycle events.</param>
    public ComponentCollection(Func<IEnumerable<IAsyncComponent>> items, IProfilingLogger profilingLogger, ILogger<ComponentCollection> logger)
        : base(items)
    {
        _profilingLogger = profilingLogger;
        _logger = logger;
    }

    /// <summary>
    /// Initializes all components in the collection.
    /// </summary>
    /// <param name="isRestarting">If set to <c>true</c> indicates Umbraco is restarting.</param>
    /// <param name="cancellationToken">The cancellation token. Cancellation indicates that the start process has been aborted.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        using (_profilingLogger.IsEnabled(Logging.LogLevel.Debug) is false
            ? null
            : _profilingLogger.DebugDuration<ComponentCollection>($"Initializing. (log components when >{LogThresholdMilliseconds}ms)", "Initialized."))
        {
            foreach (IAsyncComponent component in this)
            {
                Type componentType = component.GetType();

                using (_profilingLogger.IsEnabled(Logging.LogLevel.Debug) is false
                    ? null :
                    _profilingLogger.DebugDuration<ComponentCollection>($"Initializing {componentType.FullName}.", $"Initialized {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                {
                    await component.InitializeAsync(isRestarting, cancellationToken);
                }
            }
        }
    }

    /// <summary>
    /// Terminates all components in the collection in reverse order.
    /// </summary>
    /// <param name="isRestarting">If set to <c>true</c> indicates Umbraco is restarting.</param>
    /// <param name="cancellationToken">The cancellation token. Cancellation indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        using (!_profilingLogger.IsEnabled(Logging.LogLevel.Debug)
            ? null
            : _profilingLogger.DebugDuration<ComponentCollection>($"Terminating. (log components when >{LogThresholdMilliseconds}ms)", "Terminated."))
        {
            // terminate components in reverse order
            foreach (IAsyncComponent component in this.Reverse())
            {
                Type componentType = component.GetType();

                using (_profilingLogger.IsEnabled(Logging.LogLevel.Debug) is false
                    ? null
                    : _profilingLogger.DebugDuration<ComponentCollection>($"Terminating {componentType.FullName}.", $"Terminated {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                {
                    try
                    {
                        await component.TerminateAsync(isRestarting, cancellationToken);
                        (component as IDisposable)?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while terminating component {ComponentType}.", componentType.FullName);
                    }
                }
            }
        }
    }
}
