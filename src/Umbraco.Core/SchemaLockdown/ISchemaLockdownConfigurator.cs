namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Denies permission verbs while the schema lockdown restrictions are built at start-up. Nothing is locked unless a
/// configurator denies it.
/// </summary>
/// <example>
/// Locking changes to document types and data types:
/// <code>
/// <![CDATA[
/// public class LockContentModellingConfigurator : ISchemaLockdownConfigurator
/// {
///     public void Configure(ISchemaRestrictionsBuilder builder)
///         => builder.Deny(
///             Constants.Permissions.DocumentType.Create,
///             Constants.Permissions.DocumentType.Update,
///             Constants.Permissions.DocumentType.Delete,
///             Constants.Permissions.DataType.Create,
///             Constants.Permissions.DataType.Update,
///             Constants.Permissions.DataType.Delete);
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
