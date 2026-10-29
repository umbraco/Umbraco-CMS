using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Builds the <see cref="SchemaLockdownConfiguratorCollection"/>.
/// </summary>
/// <remarks>
/// Removing a configurator is how a site overrides what a package decided.
/// </remarks>
public class SchemaLockdownConfiguratorCollectionBuilder : SetCollectionBuilderBase<SchemaLockdownConfiguratorCollectionBuilder, SchemaLockdownConfiguratorCollection, ISchemaLockdownConfigurator>
{
    /// <inheritdoc />
    protected override SchemaLockdownConfiguratorCollectionBuilder This => this;
}
