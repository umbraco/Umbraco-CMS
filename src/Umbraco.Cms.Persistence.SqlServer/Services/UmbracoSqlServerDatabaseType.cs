using Microsoft.Data.SqlClient;
using NPoco;
using NPoco.DatabaseTypes;
using NPoco.SqlServer;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
/// Custom SQL Server database type that ensures FK constraints remain trusted
/// during bulk insert operations by using <see cref="SqlBulkCopyOptions.CheckConstraints"/>.
/// </summary>
/// <remarks>
/// NPoco's default <see cref="SqlServerDatabaseType.InsertBulk{T}"/> and <see cref="SqlServerDatabaseType.InsertBulkAsync{T}"/> uses
/// <see cref="SqlBulkCopyOptions.Default"/> which bypasses FK validation during <see cref="SqlBulkCopy"/> operations, causing
/// SQL Server to mark constraints as untrusted (<c>is_not_trusted = 1</c>).
/// Untrusted constraints prevent the query optimizer from using them for join elimination and cardinality estimation.
/// </remarks>
internal sealed class UmbracoSqlServerDatabaseType : SqlServer2012DatabaseType
{
    /// <inheritdoc />
    public override void InsertBulk<T>(IDatabase db, IEnumerable<T> pocos, InsertBulkOptions? options)
        => SqlBulkCopyHelper.BulkInsert(db, pocos, SqlBulkCopyOptions.CheckConstraints, options);

    /// <inheritdoc />
    public override Task InsertBulkAsync<T>(IDatabase db, IEnumerable<T> pocos, InsertBulkOptions? options, CancellationToken cancellationToken = default)
        => SqlBulkCopyHelper.BulkInsertAsync(db, pocos, SqlBulkCopyOptions.CheckConstraints, options ?? new InsertBulkOptions(), cancellationToken);
}
