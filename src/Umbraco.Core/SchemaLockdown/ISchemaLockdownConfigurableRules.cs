namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// The writable view of the schema lockdown rules, handed to each <see cref="ISchemaLockdownConfigurator"/> while
/// the rules are being built.
/// </summary>
/// <remarks>
/// It extends <see cref="ISchemaLockdownRules"/> because a configurator may consult the decisions already made by
/// the configurators that ran before it, and only then decide what to write.
/// </remarks>
public interface ISchemaLockdownConfigurableRules : ISchemaLockdownRules
{
    /// <summary>
    /// Permits the supplied operation on the supplied entity type.
    /// </summary>
    void Allow(string entityType, SchemaOperation operation);

    /// <summary>
    /// Denies the supplied operation on the supplied entity type.
    /// </summary>
    void Block(string entityType, SchemaOperation operation);

    /// <summary>
    /// Denies every operation on the supplied entity type that is not a read.
    /// </summary>
    /// <remarks>
    /// This is the way an <see cref="ISchemaLockdownConfigurator"/> should lock an entity type. Denying
    /// <see cref="SchemaOperation.Create"/>, <see cref="SchemaOperation.Update"/> and
    /// <see cref="SchemaOperation.Delete"/> individually leaves <see cref="SchemaOperation.Unknown"/> permitted,
    /// so an endpoint whose operation could not be classified would still get through.
    /// </remarks>
    void BlockMutations(string entityType);
}
