using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
/// Builds a <see cref="ComponentCollection" />.
/// </summary>
public class ComponentCollectionBuilder : OrderedCollectionBuilderBase<ComponentCollectionBuilder, ComponentCollection, IAsyncComponent>
{
    private const int LogThresholdMilliseconds = 100;

    /// <inheritdoc />
    protected override ComponentCollectionBuilder This => this;

    /// <inheritdoc />
    protected override IEnumerable<IAsyncComponent> CreateItems(IServiceProvider factory)
    {
        IProfilingLogger logger = factory.GetRequiredService<IProfilingLogger>();

        using (logger.IsEnabled(Logging.LogLevel.Debug) is false
            ? null
            : logger.DebugDuration<ComponentCollectionBuilder>($"Creating components. (log when >{LogThresholdMilliseconds}ms)", "Created."))
        {
            return base.CreateItems(factory);
        }
    }

    /// <inheritdoc />
    protected override IAsyncComponent CreateItem(IServiceProvider factory, Type itemType)
    {
        IProfilingLogger logger = factory.GetRequiredService<IProfilingLogger>();

        using (logger.IsEnabled(Logging.LogLevel.Debug) is false
            ? null :
            logger.DebugDuration<ComponentCollectionBuilder>($"Creating {itemType.FullName}.", $"Created {itemType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
        {
            return base.CreateItem(factory, itemType);
        }
    }
}
