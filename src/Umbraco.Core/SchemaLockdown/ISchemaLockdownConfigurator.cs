namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Adjusts the schema lockdown restrictions while they are being built at start-up.
/// </summary>
/// <remarks>
/// Registering a configurator is the only way an entity type comes under lockdown: nothing is locked until one
/// says so. Every registered configurator runs against the same restrictions, and because a denial cannot be lifted
/// once made, the order they run in does not affect the result.
/// </remarks>
/// <example>
/// Locking content type and data type schema:
/// <code>
/// <![CDATA[
/// public class LockContentModellingConfigurator : ISchemaLockdownConfigurator
/// {
///     public void Configure(ISchemaRestrictionsBuilder builder)
///     {
///         builder.BlockMutations(Constants.UdiEntityType.DocumentType);
///         builder.BlockMutations(Constants.UdiEntityType.DataType);
///     }
/// }
///
/// public class LockContentModellingComposer : IComposer
/// {
///     public void Compose(IUmbracoBuilder builder)
///         => builder.SchemaLockdownConfigurators().Add<LockContentModellingConfigurator>();
/// }
/// ]]>
/// </code>
/// </example>
public interface ISchemaLockdownConfigurator
{
    /// <summary>
    /// Adjusts the supplied restrictions.
    /// </summary>
    /// <param name="builder">The restrictions being built.</param>
    void Configure(ISchemaRestrictionsBuilder builder);
}
