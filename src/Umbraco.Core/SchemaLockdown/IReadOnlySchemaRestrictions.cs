namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Answers whether schema lockdown permits a given operation on a given entity type.
/// </summary>
/// <remarks>
/// This is the read-only view of the decision table, which is what anything consulting the restrictions needs. They
/// are built once at start-up and frozen, so every consumer is answered from the same decisions.
/// </remarks>
public interface IReadOnlySchemaRestrictions
{
    /// <summary>
    /// Gets the entity types at least one operation is denied on.
    /// </summary>
    /// <remarks>
    /// Anything absent from this is permitted every operation, so this is the whole of what the restrictions have to say.
    /// It is empty until a configurator denies something, and is not the set of entity types lockdown is capable of
    /// enforcing on - that is decided by which controllers declare one.
    /// </remarks>
    IReadOnlyCollection<string> RestrictedEntityTypes { get; }

    /// <summary>
    /// Gets a value indicating whether the supplied operation is permitted on the supplied entity type.
    /// </summary>
    /// <remarks>
    /// <see cref="SchemaOperation.Read"/> is always permitted. <see cref="SchemaOperation.Unknown"/> is permitted
    /// only on an entity type absent from <see cref="RestrictedEntityTypes"/>: it may well be one of the operations
    /// denied there, and there is no way to tell which.
    /// </remarks>
    bool IsAllowed(string entityType, SchemaOperation operation);
}
