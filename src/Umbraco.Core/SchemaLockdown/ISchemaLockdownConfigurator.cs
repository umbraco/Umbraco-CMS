namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Adjusts the schema lockdown matrix while it is being built at start-up.
/// </summary>
/// <remarks>
/// Registering a configurator is the only way an entity type comes under lockdown: nothing is locked until one
/// says so. Every registered configurator runs, in collection order, against the same matrix. Later writes to a
/// cell win, so registering last is how a rule is made authoritative.
/// </remarks>
/// <example>
/// Locking every entity type schema lockdown can govern:
/// <code>
/// <![CDATA[
/// public class LockAllSchemaConfigurator : ISchemaLockdownConfigurator
/// {
///     public void Configure(SchemaLockdownMatrix matrix)
///     {
///         foreach (string entityType in SchemaEntityTypes.All)
///         {
///             matrix.BlockMutations(entityType);
///         }
///     }
/// }
///
/// public class LockAllSchemaComposer : IComposer
/// {
///     public void Compose(IUmbracoBuilder builder)
///         => builder.SchemaLockdownConfigurators().Append<LockAllSchemaConfigurator>();
/// }
/// ]]>
/// </code>
/// </example>
public interface ISchemaLockdownConfigurator
{
    /// <summary>
    /// Adjusts the supplied matrix.
    /// </summary>
    /// <param name="matrix">The matrix being built.</param>
    void Configure(SchemaLockdownMatrix matrix);
}
