namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// The decision table for schema lockdown, holding which operations are denied on which entity types.
/// </summary>
/// <remarks>
/// Every registered <see cref="ISchemaLockdownConfigurator"/> writes to it while it is being constructed, after which
/// it is frozen. Freezing is what allows the same instance to be both consulted by the authorization handler and
/// served to the backoffice without the two being able to disagree. Denials only ever accumulate, so the order the
/// configurators run in does not affect the result.
/// </remarks>
internal sealed class SchemaRestrictions : ISchemaRestrictions
{
    private readonly Dictionary<string, HashSet<SchemaOperation>> _blocked = new(StringComparer.OrdinalIgnoreCase);
    private readonly bool _frozen;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaRestrictions"/> class.
    /// </summary>
    /// <param name="configurators">The registered configurators.</param>
    public SchemaRestrictions(SchemaLockdownConfiguratorCollection configurators)
    {
        foreach (ISchemaLockdownConfigurator configurator in configurators)
        {
            configurator.Configure(this);
        }

        _frozen = true;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> RestrictedEntityTypes => _blocked.Keys.ToArray();

    /// <inheritdoc />
    public void Block(string entityType, SchemaOperation operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        // Refusing the write ahead of the frozen check keeps the rule holding whenever the write is attempted,
        // rather than leaving behind a decision that looks meaningful and is not.
        if (IsBlockable(operation) is false)
        {
            return;
        }

        if (_frozen)
        {
            throw new InvalidOperationException("The schema lockdown restrictions cannot be modified after they have been built.");
        }

        if (_blocked.TryGetValue(entityType, out HashSet<SchemaOperation>? operations) is false)
        {
            _blocked[entityType] = operations = [];
        }

        operations.Add(operation);
    }

    /// <inheritdoc />
    public void BlockMutations(string entityType)
    {
        Block(entityType, SchemaOperation.Create);
        Block(entityType, SchemaOperation.Update);
        Block(entityType, SchemaOperation.Delete);
    }

    /// <inheritdoc />
    public bool IsAllowed(string entityType, SchemaOperation operation)
    {
        if (_blocked.TryGetValue(entityType, out HashSet<SchemaOperation>? operations) is false)
        {
            return true;
        }

        // An operation that could not be classified is denied wherever anything is denied: it may well be one of
        // those, and there is no way to tell which.
        return operation != SchemaOperation.Unknown && operations.Contains(operation) is false;
    }

    // Reads are never denied, and an unclassified operation is answered by whether the entity type is restricted at
    // all. Both are decided by rule, so neither can be recorded as a decision of its own.
    private static bool IsBlockable(SchemaOperation operation)
        => operation is SchemaOperation.Create or SchemaOperation.Update or SchemaOperation.Delete;
}
