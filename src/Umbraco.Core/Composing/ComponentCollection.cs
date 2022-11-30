using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Represents the collection of <see cref="IComponent" /> implementations.
/// </summary>
public class ComponentCollection : BuilderCollectionBase<IComponent>
{
    private const int LogThresholdMilliseconds = 100;
    private readonly ILogger<ComponentCollection> _logger;

    private readonly IProfilingLogger _profilingLogger;

    public ComponentCollection(Func<IEnumerable<IComponent>> items, IProfilingLogger profilingLogger, ILogger<ComponentCollection> logger)
        : base(items)
    {
        _profilingLogger = profilingLogger;
        _logger = logger;
    }

    public void Initialize()
    {
        using (_profilingLogger.DebugDuration<ComponentCollection>(
                   $"Initializing. (log components when >{LogThresholdMilliseconds}ms)", "Initialized."))
        {
            foreach (IComponent component in this)
            {
                Type componentType = component.GetType();
                using (_profilingLogger.DebugDuration<ComponentCollection>(
                    $"Initializing {componentType.FullName}.",
                    $"Initialized {componentType.FullName}.",
                    thresholdMilliseconds: LogThresholdMilliseconds))
                {
                    component.Initialize();
                }
            }
        }
    }

    public void Terminate()
    {
        using (_profilingLogger.DebugDuration<ComponentCollection>(
                   $"Terminating. (log components when >{LogThresholdMilliseconds}ms)", "Terminated."))
        {
            // terminate components in reverse order
            foreach (IComponent component in this.Reverse())
            {
                Type componentType = component.GetType();
                using (_profilingLogger.DebugDuration<ComponentCollection>(
                    $"Terminating {componentType.FullName}.",
                    $"Terminated {componentType.FullName}.",
                    thresholdMilliseconds: LogThresholdMilliseconds))
                {
                    try
                    {
                        component.Terminate();
                        component.DisposeIfDisposable();
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
