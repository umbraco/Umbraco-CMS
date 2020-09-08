using System;
using System.Collections.Generic;
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

        protected override IEnumerable<IComponent> CreateItems(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetInstance<IProfilingLogger>();

            using (_logger.DebugDuration<ComponentCollectionBuilder>($"Creating components. (log when >{LogThresholdMilliseconds}ms)", "Created."))
            {
                return base.CreateItems(serviceProvider);
            }
        }

        protected override IComponent CreateItem(IServiceProvider serviceProvider, Type itemType)
        {
            using (_logger.DebugDuration<ComponentCollectionBuilder>($"Creating {itemType.FullName}.", $"Created {itemType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
            {
                return base.CreateItem(serviceProvider, itemType);
            }
        }
    }
}
