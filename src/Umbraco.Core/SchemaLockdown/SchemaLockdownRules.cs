namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// The decision table for schema lockdown, mapping each entity type and operation to whether it is permitted.
/// </summary>
/// <remarks>
/// Every registered <see cref="ISchemaLockdownConfigurator"/> writes to it while it is being constructed, after which
/// it is frozen. Freezing is what allows the same instance to be both consulted by the authorization handler and
/// served to the backoffice without the two being able to disagree.
/// </remarks>
public sealed class SchemaLockdownRules : ISchemaLockdownConfigurableRules
{
    private static readonly DelegateEqualityComparer<(string EntityType, SchemaOperation Operation)> CellKeyComparer =
        new(
            (x, y) => x.Operation == y.Operation
                && string.Equals(x.EntityType, y.EntityType, StringComparison.OrdinalIgnoreCase),
            x => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(x.EntityType), x.Operation));

    private readonly Dictionary<(string EntityType, SchemaOperation Operation), bool> _cells = new(CellKeyComparer);
    private readonly bool _frozen;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownRules"/> class.
    /// </summary>
    /// <param name="configurators">The registered configurators.</param>
    public SchemaLockdownRules(SchemaLockdownConfiguratorCollection configurators)
    {
        foreach (ISchemaLockdownConfigurator configurator in configurators)
        {
            configurator.Configure(this);
        }

        _frozen = true;
    }

    /// <inheritdoc />
    public void Allow(string entityType, SchemaOperation operation) => Set(entityType, operation, true);

    /// <inheritdoc />
    public void Block(string entityType, SchemaOperation operation) => Set(entityType, operation, false);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool IsAllowed(string entityType, SchemaOperation operation)
        => _cells.TryGetValue((entityType, operation), out var allowed) is false || allowed;

    // Reads are never governed, so a read cell would never be consulted. Refusing to record one - ahead of the frozen
    // check, so that it holds whenever the write is attempted - keeps that a structural guarantee rather than leaving
    // behind a cell that looks meaningful and is not.
    private void Set(string entityType, SchemaOperation operation, bool allowed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        if (operation == SchemaOperation.Read)
        {
            return;
        }

        if (_frozen)
        {
            throw new InvalidOperationException("The schema lockdown rules cannot be modified after they have been built.");
        }

        _cells[(entityType, operation)] = allowed;
    }
}
