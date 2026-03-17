using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Checks whether the database is read-only by querying database.
/// </summary>
internal sealed class DatabaseReadOnlyAccessor : IDatabaseReadOnlyAccessor
{
    private const string CacheKey = "DatabaseReadOnly";

    private readonly IScopeAccessor _scopeAccessor;
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseReadOnlyAccessor"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the ambient scope for database access.</param>
    /// <param name="appCaches">The application caches.</param>
    public DatabaseReadOnlyAccessor(IScopeAccessor scopeAccessor, AppCaches appCaches)
    {
        _scopeAccessor = scopeAccessor;
        _appCaches = appCaches;
    }

    /// <inheritdoc />
    public bool IsReadOnly() =>
        _appCaches.RuntimeCache.GetCacheItem(CacheKey, Check);

    private bool Check()
    {
        try
        {
            IScope? scope = _scopeAccessor.AmbientScope;
            if (scope is null)
            {
                return true;
            }

            var result = scope.Database.ExecuteScalar<string>(
                "SELECT CAST(DATABASEPROPERTYEX(DB_NAME(), 'Updateability') AS NVARCHAR(20))");

            return string.Equals(result, "READ_ONLY", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // Query is SQL Server specific. If it fails, like it would on SQLite
            // we assume the database is writable.
            return false;
        }
    }
}
