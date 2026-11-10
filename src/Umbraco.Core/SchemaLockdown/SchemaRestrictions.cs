namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Holds which permission verbs are denied.
/// </summary>
/// <remarks>
/// Every registered <see cref="ISchemaLockdownConfigurator"/> configures it while it's constructed, after which it is
/// frozen.
/// </remarks>
internal sealed class SchemaRestrictions : ISchemaRestrictionsBuilder
{
    private readonly HashSet<string> _denied = new(StringComparer.Ordinal);
    private readonly bool _frozen;
    private IReadOnlyCollection<string> _deniedPermissions = [];

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
    public IReadOnlyCollection<string> DeniedPermissions => _deniedPermissions;

    /// <inheritdoc />
    public void Deny(params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(permission, nameof(permissions));
        }

        if (_frozen)
        {
            throw new InvalidOperationException("The schema lockdown restrictions cannot be modified after they have been built.");
        }

        var added = false;
        foreach (var permission in permissions)
        {
            added |= _denied.Add(permission);
        }

        if (added)
        {
            // Replaced, not updated, so a configurator can keep enumerating the denied permissions while it denies more.
            _deniedPermissions = Array.AsReadOnly(_denied.ToArray());
        }
    }

    /// <inheritdoc />
    public bool IsDenied(string permission) => _denied.Contains(permission);
}
