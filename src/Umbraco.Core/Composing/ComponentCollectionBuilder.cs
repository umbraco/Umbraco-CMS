using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Builds a <see cref="ComponentCollection"/>.
    /// </summary>
    public class ComponentCollectionBuilder : OrderedCollectionBuilderBase<ComponentCollectionBuilder, ComponentCollection, IComponent>
    {
        private const int LogThresholdMilliseconds = 100;

        private IProfilingLogger _logger;

        public ComponentCollectionBuilder()
        { }

        protected override ComponentCollectionBuilder This => this;

        protected override IEnumerable<IComponent> CreateItems(IServiceProvider factory)
        {
            _logger = factory.GetRequiredService<IProfilingLogger>();

            using (_logger.DebugDuration<ComponentCollectionBuilder>($"Creating components. (log when >{LogThresholdMilliseconds}ms)", "Created."))
            {
                return base.CreateItems(factory);
            }
        }

        protected override IComponent CreateItem(IServiceProvider factory, Type itemType)
        {
            using (_logger.DebugDuration<ComponentCollectionBuilder>($"Creating {itemType.FullName}.", $"Created {itemType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
            {
                return base.CreateItem(factory, itemType);
            }
        }
    }
}
