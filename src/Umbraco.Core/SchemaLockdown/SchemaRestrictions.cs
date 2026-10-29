namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Holds which operations are denied on which entity types.
/// </summary>
/// <remarks>
/// Every registered <see cref="ISchemaLockdownConfigurator"/> configures it while it's constructed, after which it is
/// frozen.
/// </remarks>
internal sealed class SchemaRestrictions : ISchemaRestrictionsBuilder
{
    private readonly Dictionary<string, HashSet<SchemaOperation>> _blocked = new(StringComparer.OrdinalIgnoreCase);
    private readonly bool _frozen;
    private IReadOnlyCollection<string> _restrictedEntityTypes = [];

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
    public IReadOnlyCollection<string> RestrictedEntityTypes => _restrictedEntityTypes;

    /// <inheritdoc />
    public void Block(string entityType, params SchemaOperation[] operations)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        foreach (SchemaOperation operation in operations)
        {
            Block(entityType, operation);
        }
    }

    /// <inheritdoc />
    public void BlockMutations(string entityType)
        => Block(entityType, SchemaOperation.Create, SchemaOperation.Update, SchemaOperation.Delete);

    /// <inheritdoc />
    public bool IsRestricted(string entityType) => _blocked.ContainsKey(entityType);

    /// <inheritdoc />
    public bool IsAllowed(string entityType, SchemaOperation operation)
    {
        if (_blocked.TryGetValue(entityType, out HashSet<SchemaOperation>? operations) is false)
        {
            return true;
        }

        return operation != SchemaOperation.Unknown && operations.Contains(operation) is false;
    }

    private void Block(string entityType, SchemaOperation operation)
    {
        if (IsBlockable(operation) is false)
        {
            return;
        }

        if (_frozen)
        {
            throw new InvalidOperationException("The schema lockdown restrictions cannot be modified after they have been built.");
        }

        if (_blocked.TryGetValue(entityType, out HashSet<SchemaOperation>? blocked) is false)
        {
            _blocked[entityType] = blocked = [];

            // Replaced, not updated, so a configurator can keep enumerating the entity types while it blocks more.
            _restrictedEntityTypes = Array.AsReadOnly(_blocked.Keys.ToArray());
        }

        blocked.Add(operation);
    }

    private static bool IsBlockable(SchemaOperation operation)
        => operation is SchemaOperation.Create or SchemaOperation.Update or SchemaOperation.Delete;
}
