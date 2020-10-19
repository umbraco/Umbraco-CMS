﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents the collection of <see cref="IComponent"/> implementations.
    /// </summary>
    public class ComponentCollection : BuilderCollectionBase<IComponent>
    {
        private const int LogThresholdMilliseconds = 100;

        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<ComponentCollection> _logger;

        public ComponentCollection(IEnumerable<IComponent> items, IProfilingLogger profilingLogger, ILogger<ComponentCollection> logger)
            : base(items)
        {
            _profilingLogger = profilingLogger;
            _logger = logger;
        }

        public void Initialize()
        {
            using (_profilingLogger.DebugDuration<ComponentCollection>($"Initializing. (log components when >{LogThresholdMilliseconds}ms)", "Initialized."))
            {
                foreach (var component in this)
                {
                    var componentType = component.GetType();
                    using (_profilingLogger.DebugDuration<ComponentCollection>($"Initializing {componentType.FullName}.", $"Initialized {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        component.Initialize();
                    }
                }
            }
        }

        public void Terminate()
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
}
