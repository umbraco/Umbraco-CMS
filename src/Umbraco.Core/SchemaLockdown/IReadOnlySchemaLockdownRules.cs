namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Answers whether schema lockdown permits a given operation on a given entity type.
/// </summary>
/// <remarks>
/// This is the read-only view of the decision table, which is what anything consulting the rules needs. The rules
/// are built once at start-up and frozen, so every consumer is answered from the same decisions.
/// </remarks>
public interface IReadOnlySchemaLockdownRules
{
    /// <summary>
    /// Gets the entity types the rules hold a decision for.
    /// </summary>
    /// <remarks>
    /// Anything absent from this is permitted every operation, so this is the whole of what the rules have to say.
    /// It is empty until a configurator writes something.
    /// </remarks>
    IReadOnlyCollection<string> GovernedEntityTypes { get; }

    /// <summary>
    /// Gets a value indicating whether the supplied operation is permitted on the supplied entity type.
    /// </summary>
    bool IsAllowed(string entityType, SchemaOperation operation);
}
