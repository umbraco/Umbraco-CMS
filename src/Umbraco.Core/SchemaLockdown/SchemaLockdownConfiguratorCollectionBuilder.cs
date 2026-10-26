using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Builds the <see cref="SchemaLockdownConfiguratorCollection"/>.
/// </summary>
/// <remarks>
/// This is a set rather than an ordered collection because a denial cannot be lifted once made, so the order the
/// configurators run in cannot affect the result. Removing one is how a site overrides what a package decided.
/// </remarks>
public class SchemaLockdownConfiguratorCollectionBuilder : SetCollectionBuilderBase<SchemaLockdownConfiguratorCollectionBuilder, SchemaLockdownConfiguratorCollection, ISchemaLockdownConfigurator>
{
    /// <inheritdoc />
    protected override SchemaLockdownConfiguratorCollectionBuilder This => this;
}
