using Microsoft.Data.SqlClient;
using NPoco;
using NPoco.SqlServer;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.SqlServer;

public static class NPocoSqlServerDatabaseExtensions
{
    /// <summary>
    ///     Configures NPoco's SqlBulkCopyHelper to use the correct SqlConnection and SqlTransaction instances from the
    ///     underlying RetryDbConnection and ProfiledDbTransaction
    /// </summary>
    /// <remarks>
    ///     This is required to use NPoco's own <see cref="Database.InsertBulk{T}(IEnumerable{T})" /> method because we use
    ///     wrapped DbConnection and DbTransaction instances.
    ///     NPoco's InsertBulk method only caters for efficient bulk inserting records for Sql Server, it does not cater for
    ///     bulk inserting of records for
    ///     any other database type and in which case will just insert records one at a time.
    ///     NPoco's InsertBulk method also deals with updating the passed in entity's PK/ID once it's inserted whereas our own
    ///     BulkInsertRecords methods
    ///     do not handle this scenario.
    /// </remarks>
    public static void ConfigureNPocoBulkExtensions()
    {
        SqlBulkCopyHelper.SqlConnectionResolver = NPocoDatabaseExtensions.GetTypedConnection<SqlConnection>;
        SqlBulkCopyHelper.SqlTransactionResolver = NPocoDatabaseExtensions.GetTypedTransaction<SqlTransaction>;
    }
}
