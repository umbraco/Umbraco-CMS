namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// The decision table for schema lockdown, mapping each entity type and operation to whether it is permitted.
/// </summary>
/// <remarks>
/// Built and mutated during start-up, then frozen. Freezing is what allows the same instance to be both consulted
/// by the request filter and served to the backoffice without the two being able to disagree.
/// </remarks>
public sealed class SchemaLockdownMatrix
{
    private static readonly DelegateEqualityComparer<(string EntityType, SchemaOperation Operation)> CellKeyComparer =
        new(
            (x, y) => x.Operation == y.Operation
                && string.Equals(x.EntityType, y.EntityType, StringComparison.OrdinalIgnoreCase),
            x => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(x.EntityType), x.Operation));

    private readonly Dictionary<(string EntityType, SchemaOperation Operation), bool> _cells = new(CellKeyComparer);
    private bool _frozen;

    /// <summary>
    /// Permits the supplied operation on the supplied entity type.
    /// </summary>
    public void Allow(string entityType, SchemaOperation operation) => Set(entityType, operation, true);

    /// <summary>
    /// Denies the supplied operation on the supplied entity type.
    /// </summary>
    public void Block(string entityType, SchemaOperation operation) => Set(entityType, operation, false);

    /// <summary>
    /// Denies every operation on the supplied entity type that is not a read.
    /// </summary>
    /// <remarks>
    /// This is the way an <see cref="ISchemaLockdownConfigurator"/> should lock an entity type. Denying
    /// <see cref="SchemaOperation.Create"/>, <see cref="SchemaOperation.Update"/> and
    /// <see cref="SchemaOperation.Delete"/> individually leaves <see cref="SchemaOperation.Unknown"/> permitted,
    /// so an endpoint whose operation could not be classified would still get through.
    /// </remarks>
    public void BlockMutations(string entityType)
    {
        foreach (SchemaOperation operation in Enum.GetValues<SchemaOperation>())
        {
            if (operation != SchemaOperation.Read)
            {
                Block(entityType, operation);
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the supplied operation is permitted on the supplied entity type.
    /// </summary>
    public bool IsAllowed(string entityType, SchemaOperation operation)
        => _cells.TryGetValue((entityType, operation), out var allowed) is false || allowed;

    /// <summary>
    /// Prevents any further mutation.
    /// </summary>
    /// <remarks>
    /// Only <see cref="SchemaLockdownMatrixAccessor"/> calls this, after every configurator has run. If an
    /// <see cref="ISchemaLockdownConfigurator"/> could call it too, one configurator could freeze the matrix before
    /// later configurators in the same build get a chance to configure it.
    /// </remarks>
    internal void Freeze() => _frozen = true;

    /// <summary>
    /// Gets the full decision table.
    /// </summary>
    internal IReadOnlyDictionary<string, IReadOnlyDictionary<SchemaOperation, bool>> Snapshot()
        => SchemaEntityTypes.All.ToDictionary(
            entityType => entityType,
            entityType => (IReadOnlyDictionary<SchemaOperation, bool>)Enum.GetValues<SchemaOperation>()
                .ToDictionary(operation => operation, operation => IsAllowed(entityType, operation)));

    private void Set(string entityType, SchemaOperation operation, bool allowed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        if (_frozen)
        {
            throw new InvalidOperationException("The schema lockdown matrix cannot be modified after it has been frozen.");
        }

        _cells[(entityType, operation)] = allowed;
    }
}
