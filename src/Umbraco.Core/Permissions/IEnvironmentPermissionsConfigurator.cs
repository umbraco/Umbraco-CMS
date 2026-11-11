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
