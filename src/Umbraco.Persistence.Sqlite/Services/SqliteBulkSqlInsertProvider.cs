using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Persistence.Sqlite.Services;

/// <summary>
/// Implements <see cref="IBulkSqlInsertProvider"/> for SQLite.
/// </summary>
public class SqliteBulkSqlInsertProvider : IBulkSqlInsertProvider
{
    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <inheritdoc />
    public int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records) => throw new NotImplementedException();
}
