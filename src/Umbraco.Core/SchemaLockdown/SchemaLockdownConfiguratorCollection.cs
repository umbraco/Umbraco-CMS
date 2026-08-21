using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Represents a collection of <see cref="ISchemaLockdownConfigurator"/> instances.
/// </summary>
public class SchemaLockdownConfiguratorCollection : BuilderCollectionBase<ISchemaLockdownConfigurator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownConfiguratorCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that provides the configurators.</param>
    public SchemaLockdownConfiguratorCollection(Func<IEnumerable<ISchemaLockdownConfigurator>> items)
        : base(items)
    {
    }
}
