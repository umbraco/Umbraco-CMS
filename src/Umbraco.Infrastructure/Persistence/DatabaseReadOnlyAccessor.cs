using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Checks whether the database is read-only by querying database.
/// </summary>
internal sealed class DatabaseReadOnlyAccessor : IDatabaseReadOnlyAccessor
{
    private readonly IScopeAccessor _scopeAccessor;
    private bool? _isReadOnly;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseReadOnlyAccessor"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the ambient scope for database access.</param>
    public DatabaseReadOnlyAccessor(IScopeAccessor scopeAccessor)
    {
        _scopeAccessor = scopeAccessor;
    }

    /// <inheritdoc />
    public bool IsReadOnly() =>
        _isReadOnly ??= Check();

    private bool Check()
    {
        IScope? scope = _scopeAccessor.AmbientScope;
        if (scope is null)
        {
            throw new InvalidOperationException("DatabaseReadOnlyAccessor requires an ambient scope.");
        }

        if (scope.Database.DatabaseType.IsSqlServer() is false)
        {
            // Assume writeable database if it isn't a SQL Server.
            return false;
        }

        var result = scope.Database.ExecuteScalar<string>(
            "SELECT CAST(DATABASEPROPERTYEX(DB_NAME(), 'Updateability') AS NVARCHAR(20))");

        return string.Equals(result, "READ_ONLY", StringComparison.OrdinalIgnoreCase);
    }
}
