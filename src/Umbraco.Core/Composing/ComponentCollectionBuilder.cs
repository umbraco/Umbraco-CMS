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

        protected override IEnumerable<IComponent> CreateItems(IFactory factory)
        {
            _logger = factory.GetInstance<IProfilingLogger>();

            using (_logger.DebugDuration<ComponentCollectionBuilder>($"Creating components. (log when >{LogThresholdMilliseconds}ms)", "Created."))
            {
                return base.CreateItems(factory);
            }
        }

        protected override IComponent CreateItem(IFactory factory, Type itemType)
        {
            using (_logger.DebugDuration<Composers>($"Creating {itemType.FullName}.", $"Created {itemType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
            {
                return base.CreateItem(factory, itemType);
            }
        }
    }
}
