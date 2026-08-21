using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Builds the <see cref="SchemaLockdownConfiguratorCollection"/>.
/// </summary>
public class SchemaLockdownConfiguratorCollectionBuilder : OrderedCollectionBuilderBase<SchemaLockdownConfiguratorCollectionBuilder, SchemaLockdownConfiguratorCollection, ISchemaLockdownConfigurator>
{
    /// <inheritdoc />
    protected override SchemaLockdownConfiguratorCollectionBuilder This => this;
}
