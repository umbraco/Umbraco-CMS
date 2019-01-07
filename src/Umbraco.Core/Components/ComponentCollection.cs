using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents the collection of <see cref="IComponent"/> implementations.
    /// </summary>
    public class ComponentCollection : BuilderCollectionBase<IComponent>
    {
        private const int LogThresholdMilliseconds = 100;

        private readonly IProfilingLogger _logger;

        public ComponentCollection(IEnumerable<IComponent> items, IProfilingLogger logger)
            : base(items)
        {
            _logger = logger;
        }

        public void Terminate()
        {
            using (_logger.DebugDuration<ComponentCollection>($"Terminating. (log components when >{LogThresholdMilliseconds}ms)", "Terminated."))
            {
                foreach (var component in this.Reverse()) // terminate components in reverse order
                {
                    var componentType = component.GetType();
                    using (_logger.DebugDuration<ComponentCollection>($"Terminating {componentType.FullName}.", $"Terminated {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        component.DisposeIfDisposable();
                    }
                }
            }
        }
    }
}
