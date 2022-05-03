using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Composing
{
    /// <summary>
    /// Represents the collection of <see cref="IComponent"/> implementations.
    /// </summary>
    public class ComponentCollection : BuilderCollectionBase<IComponent>
    {
        private const int LogThresholdMilliseconds = 100;

        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<ComponentCollection> _logger;

        public ComponentCollection(Func<IEnumerable<IComponent>> items, IProfilingLogger profilingLogger, ILogger<ComponentCollection> logger)
            : base(items)
        {
            _profilingLogger = profilingLogger;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            using (_profilingLogger.DebugDuration<ComponentCollection>($"Initializing. (log components when >{LogThresholdMilliseconds}ms)", "Initialized."))
            {
                foreach (var component in this)
                {
                    var componentType = component.GetType();
                    using (_profilingLogger.DebugDuration<ComponentCollection>($"Initializing {componentType.FullName}.", $"Initialized {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        await component.InitializeAsync(cancellationToken);
                    }
                }
            }
        }

        public async Task TerminateAsync(CancellationToken cancellationToken)
        {
            using (_profilingLogger.DebugDuration<ComponentCollection>($"Terminating. (log components when >{LogThresholdMilliseconds}ms)", "Terminated."))
            {
                foreach (var component in this.Reverse()) // terminate components in reverse order
                {
                    var componentType = component.GetType();
                    using (_profilingLogger.DebugDuration<ComponentCollection>($"Terminating {componentType.FullName}.", $"Terminated {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        try
                        {
                            await component.TerminateAsync(cancellationToken);
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
}
