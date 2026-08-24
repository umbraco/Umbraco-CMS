namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Adjusts the schema lockdown rules while they are being built at start-up.
/// </summary>
/// <remarks>
/// Registering a configurator is the only way an entity type comes under lockdown: nothing is locked until one
/// says so. Every registered configurator runs, in collection order, against the same rules. Later writes to a
/// cell win, so registering last is how a rule is made authoritative.
/// </remarks>
/// <example>
/// Locking every entity type schema lockdown can govern:
/// <code>
/// <![CDATA[
/// public class LockAllSchemaConfigurator : ISchemaLockdownConfigurator
/// {
///     public void Configure(ISchemaLockdownConfigurableRules rules)
///     {
///         foreach (string entityType in SchemaEntityTypes.All)
///         {
///             rules.BlockMutations(entityType);
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
    /// Adjusts the supplied rules.
    /// </summary>
    /// <param name="rules">The rules being built.</param>
    void Configure(ISchemaLockdownConfigurableRules rules);
}
