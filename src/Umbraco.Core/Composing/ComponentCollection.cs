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

    public ComponentCollection(Func<IEnumerable<IAsyncComponent>> items, IProfilingLogger profilingLogger, ILogger<ComponentCollection> logger)
        : base(items)
    {
        _profilingLogger = profilingLogger;
        _logger = logger;
    }

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
