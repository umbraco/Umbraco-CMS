namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// The writable view of the schema lockdown rules, handed to each <see cref="ISchemaLockdownConfigurator"/> while
/// the rules are being built.
/// </summary>
/// <remarks>
/// It extends <see cref="IReadOnlySchemaLockdownRules"/> because a configurator may consult the decisions already made by
/// the configurators that ran before it, and only then decide what to write. There is deliberately no way to permit
/// something: everything is permitted until a configurator denies it, and a denial cannot then be lifted. A site that
/// disagrees with a package's configurator removes it from the collection rather than amending what it decided.
/// </remarks>
public interface ISchemaLockdownRules : IReadOnlySchemaLockdownRules
{
    /// <summary>
    /// Denies the supplied operation on the supplied entity type.
    /// </summary>
    /// <remarks>
    /// Only <see cref="SchemaOperation.Create"/>, <see cref="SchemaOperation.Update"/> and
    /// <see cref="SchemaOperation.Delete"/> can be denied; the others are answered by rule and denying one does
    /// nothing. See <see cref="IReadOnlySchemaLockdownRules.IsAllowed"/>.
    /// </remarks>
    void Block(string entityType, SchemaOperation operation);

    /// <summary>
    /// Denies every operation on the supplied entity type that is not a read.
    /// </summary>
    void BlockMutations(string entityType);
}
