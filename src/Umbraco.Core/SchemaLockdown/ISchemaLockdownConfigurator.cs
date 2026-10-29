namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Denies schema operations while the schema lockdown restrictions are built at start-up. Nothing is locked unless a
/// configurator denies it.
/// </summary>
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
