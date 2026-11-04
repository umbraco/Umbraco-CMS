namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Answers whether schema lockdown permits an operation on an entity type.
/// </summary>
/// <remarks>
/// The restrictions are built once at start-up through <see cref="ISchemaRestrictionsBuilder"/> and can't change
/// afterwards.
/// </remarks>
public interface ISchemaRestrictions
{
    /// <summary>
    /// Gets the entity types at least one operation is denied on. Any other entity type is permitted every operation.
    /// </summary>
    IReadOnlyCollection<string> RestrictedEntityTypes { get; }

    /// <summary>
    /// Gets a value indicating whether at least one operation is denied on the supplied entity type.
    /// </summary>
    /// <param name="entityType">The entity type, for example one of <see cref="Constants.UdiEntityType"/>.</param>
    /// <returns><c>true</c> if at least one operation is denied on the entity type; otherwise <c>false</c>.</returns>
    bool IsRestricted(string entityType);

    /// <summary>
    /// Gets a value indicating whether the supplied operation is permitted on the supplied entity type.
    /// </summary>
    /// <remarks>
    /// <see cref="SchemaOperation.Read"/> is always permitted. <see cref="SchemaOperation.Unknown"/> is denied on any
    /// restricted entity type, as it can't be told apart from the operations denied there.
    /// </remarks>
    /// <param name="entityType">The entity type, for example one of <see cref="Constants.UdiEntityType"/>.</param>
    /// <param name="operation">The operation to check.</param>
    /// <returns><c>true</c> if the operation is permitted on the entity type; otherwise <c>false</c>.</returns>
    bool IsAllowed(string entityType, SchemaOperation operation);
}
