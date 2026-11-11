namespace Umbraco.Cms.Core.Permissions;

/// <summary>
/// Denies permission verbs while the environment permissions are built at start-up. Nothing is locked unless a
/// configurator denies it.
/// </summary>
/// <example>
/// Locking changes to document types and data types:
/// <code>
/// <![CDATA[
/// public class LockContentModellingConfigurator : IEnvironmentPermissionsConfigurator
/// {
///     public void Configure(IEnvironmentPermissionsBuilder builder)
///         => builder.Deny(
///             Constants.Permissions.Entities.DocumentType.Create,
///             Constants.Permissions.Entities.DocumentType.Update,
///             Constants.Permissions.Entities.DocumentType.Delete,
///             Constants.Permissions.Entities.DataType.Create,
///             Constants.Permissions.Entities.DataType.Update,
///             Constants.Permissions.Entities.DataType.Delete);
/// }
///
/// public class LockContentModellingComposer : IComposer
/// {
///     public void Compose(IUmbracoBuilder builder)
///         => builder.EnvironmentPermissionsConfigurators().Add<LockContentModellingConfigurator>();
/// }
/// ]]>
/// </code>
/// </example>
public interface IEnvironmentPermissionsConfigurator
{
    /// <summary>
    /// Adjusts the supplied environment permissions.
    /// </summary>
    /// <param name="builder">The environment permissions being built.</param>
    void Configure(IEnvironmentPermissionsBuilder builder);
}
