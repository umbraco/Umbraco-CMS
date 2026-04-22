using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Checks whether the database is read-only by querying database.
/// </summary>
internal sealed class DatabaseReadOnlyAccessor : IDatabaseReadOnlyAccessor
{
    private readonly IScopeProvider _scopeProvider;
    private bool? _isReadOnly;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseReadOnlyAccessor"/> class.
    /// </summary>
    /// <param name="scopeProvider">Provides scope creation for database access.</param>
    public DatabaseReadOnlyAccessor(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc />
    public bool IsReadOnly() =>
        _isReadOnly ??= Check();

    private bool Check()
    {
        using IScope scope = _scopeProvider.CreateScope(autoComplete: true);

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
