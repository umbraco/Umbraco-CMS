using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using NPoco;
using StackExchange.Profiling.Data;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to NPoco Database class.
/// </summary>
public static partial class NPocoDatabaseExtensions
{
    /// <summary>
    ///     Iterates over the result of a paged data set with a db reader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database">The database to query.</param>
    /// <param name="pageSize">
    ///     The number of rows to load per page
    /// </param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="sqlCount">
    ///     Specify a custom Sql command to get the total count, if null is specified than the
    ///     auto-generated sql count will be used
    /// </param>
    /// <returns>An enumerable of rows from the paged query.</returns>
    /// <remarks>
    ///     NPoco's normal Page returns a List{T} but sometimes we don't want all that in memory and instead want to
    ///     iterate over each row with a reader using Query vs Fetch.
    /// </remarks>
    public static IEnumerable<T> QueryPaged<T>(this IDatabase database, long pageSize, Sql sql, Sql? sqlCount)
    {
        var sqlString = sql.SQL;
        var sqlArgs = sql.Arguments;

        int? itemCount = null;
        long pageIndex = 0;
        do
        {
            // Get the paged queries
            database.BuildPageQueries<T>(pageIndex * pageSize, pageSize, sqlString, ref sqlArgs, out var generatedSqlCount, out var sqlPage);

            // get the item count once
            if (itemCount == null)
            {
                itemCount = database.ExecuteScalar<int>(
                    sqlCount?.SQL ?? generatedSqlCount,
                    sqlCount?.Arguments ?? sqlArgs);
            }

            pageIndex++;

            // iterate over rows without allocating all items to memory (Query vs Fetch)
            foreach (T row in database.Query<T>(sqlPage, sqlArgs))
            {
                yield return row;
            }
        }
        while (pageIndex * pageSize < itemCount);
    }

    /// <summary>
    ///     Iterates over the result of a paged data set with a db reader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database">The database to query.</param>
    /// <param name="pageSize">
    ///     The number of rows to load per page
    /// </param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>An enumerable of rows from the paged query.</returns>
    /// <remarks>
    ///     NPoco's normal Page returns a List{T} but sometimes we don't want all that in memory and instead want to
    ///     iterate over each row with a reader using Query vs Fetch.
    /// </remarks>
    public static IEnumerable<T> QueryPaged<T>(this IDatabase database, long pageSize, Sql sql) =>
        database.QueryPaged<T>(pageSize, sql, null);

    // NOTE
    //
    // proper way to do it with TSQL and SQLCE
    //   IF EXISTS (SELECT ... FROM table WITH (UPDLOCK,HOLDLOCK)) WHERE ...)
    //   BEGIN
    //     UPDATE table SET ... WHERE ...
    //   END
    //   ELSE
    //   BEGIN
    //     INSERT INTO table (...) VALUES (...)
    //   END
    //
    // works in READ COMMITED, TSQL & SQLCE lock the constraint even if it does not exist, so INSERT is OK
    //
    // TODO: use the proper database syntax, not this kludge

    /// <summary>
    ///     Safely inserts a record, or updates if it exists, based on a unique constraint.
    /// </summary>
    /// <param name="db"></param>
    /// <param name="poco"></param>
    /// <returns>
    ///     The action that executed, either an insert or an update. If an insert occurred and a PK value got generated, the
    ///     poco object
    ///     passed in will contain the updated value.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         We cannot rely on database-specific options because SQLCE
    ///         does not support any of them. Ideally this should be achieved with proper transaction isolation levels but that
    ///         would mean revisiting
    ///         isolation levels globally. We want to keep it simple for the time being and manage it manually.
    ///     </para>
    ///     <para>We handle it by trying to update, then insert, etc. until something works, or we get bored.</para>
    ///     <para>
    ///         Note that with proper transactions, if T2 begins after T1 then we are sure that the database will contain T2's
    ///         value
    ///         once T1 and T2 have completed. Whereas here, it could contain T1's value.
    ///     </para>
    /// </remarks>
    [Obsolete("Use InsertOrUpdateAsync instead")]
    public static RecordPersistenceType InsertOrUpdate<T>(this IUmbracoDatabase db, T poco)
        where T : class =>
        db.InsertOrUpdate(poco, null, null);

    /// <summary>
    ///     Safely inserts a record, or updates if it exists, based on a unique constraint.
    /// </summary>
    /// <param name="db"></param>
    /// <param name="poco"></param>
    /// <param name="updateArgs"></param>
    /// <param name="updateCommand">If the entity has a composite key they you need to specify the update command explicitly</param>
    /// <returns>
    ///     The action that executed, either an insert or an update. If an insert occurred and a PK value got generated, the
    ///     poco object
    ///     passed in will contain the updated value.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         We cannot rely on database-specific options because SQLCE
    ///         does not support any of them. Ideally this should be achieved with proper transaction isolation levels but that
    ///         would mean revisiting
    ///         isolation levels globally. We want to keep it simple for the time being and manage it manually.
    ///     </para>
    ///     <para>We handle it by trying to update, then insert, etc. until something works, or we get bored.</para>
    ///     <para>
    ///         Note that with proper transactions, if T2 begins after T1 then we are sure that the database will contain T2's
    ///         value
    ///         once T1 and T2 have completed. Whereas here, it could contain T1's value.
    ///     </para>
    /// </remarks>
    public static async Task<RecordPersistenceType> InsertOrUpdateAsync<T>(
        this IUmbracoDatabase db,
        T poco,
        string? updateCommand,
        object? updateArgs)
        where T : class
    {
        if (poco == null)
        {
            throw new ArgumentNullException(nameof(poco));
        }

        // TODO: NPoco has a Save method that works with the primary key
        //  in any case, no point trying to update if there's no primary key!

        // try to update
        var rowCount = updateCommand.IsNullOrWhiteSpace() || updateArgs is null
            ? await db.UpdateAsync(poco)
            : db.Update<T>(updateCommand!, updateArgs);
        if (rowCount > 0)
        {
            return RecordPersistenceType.Update;
        }

        // failed: does not exist, need to insert
        // RC1 race cond here: another thread may insert a record with the same constraint
        var i = 0;
        while (i++ < 4)
        {
            try
            {
                // try to insert
                await db.InsertAsync(poco);
                return RecordPersistenceType.Insert;
            }
            catch (DbException)
            {
                // assuming all db engines will throw SQLException exception
                // failed: exists (due to race cond RC1)
                // RC2 race cond here: another thread may remove the record

                // try to update
                rowCount = updateCommand.IsNullOrWhiteSpace() || updateArgs is null
                    ? await db.UpdateAsync(poco)
                    : db.Update<T>(updateCommand!, updateArgs);
                if (rowCount > 0)
                {
                    return RecordPersistenceType.Update;
                }

                // failed: does not exist (due to race cond RC2), need to insert
                // loop
            }
        }

        // this can go on forever... have to break at some point and report an error.
        throw new DataException("Record could not be inserted or updated.");
    }

    /// <summary>
    /// Safely inserts a record, or updates it if it exists, based on a unique constraint.
    /// </summary>
    /// <param name="db">The <see cref="IUmbracoDatabase"/> instance to perform the operation on.</param>
    /// <param name="poco">The record to insert or update.</param>
    /// <param name="updateCommand">An optional custom update command to use for updating the record. If null, a default update command is used.</param>
    /// <param name="updateArgs">Optional arguments for the update command.</param>
    /// <returns>
    /// The action that executed, either an insert or an update. If an insert occurred and a primary key value was generated, the <paramref name="poco"/> object will be updated with the new value.
    /// </returns>
    /// <remarks>
    /// <para>
    ///     This method does not rely on database-specific upsert options because SQLCE does not support them. Instead, it attempts to update, then insert, until successful.
    ///     Note that transaction isolation is managed manually, and in some concurrency scenarios, the final value in the database may not be deterministic.
    /// </para>
    /// </remarks>
    public static RecordPersistenceType InsertOrUpdate<T>(
        this IUmbracoDatabase db,
        T poco,
        string? updateCommand,
        object? updateArgs)
        where T : class =>
        db.InsertOrUpdateAsync(poco, updateCommand, updateArgs).GetAwaiter().GetResult();

    /// <summary>
    ///     This will escape single @ symbols for npoco values so it doesn't think it's a parameter
    /// </summary>
    /// <param name="value">The string containing @ symbols to escape.</param>
    /// <returns>The string with single @ symbols escaped as @@.</returns>
    public static string EscapeAtSymbols(string value)
    {
        if (value.Contains('@') == false)
        {
            return value;
        }

        // this fancy regex will only match a single @ not a double, etc...
        return AtRegex().Replace(value, "@@");
    }

    [GeneratedRegex("(?<!@)@(?!@)")]
    private static partial Regex AtRegex();

    /// <summary>
    ///     Returns the underlying connection as a typed connection - this is used to unwrap the profiled mini profiler stuff
    /// </summary>
    /// <typeparam name="TConnection"></typeparam>
    /// <param name="connection">The database connection to unwrap.</param>
    /// <returns>The underlying typed connection.</returns>
    public static TConnection GetTypedConnection<TConnection>(IDbConnection? connection)
        where TConnection : class, IDbConnection
    {
        IDbConnection? c = connection;
        for (; ;)
        {
            switch (c)
            {
                case TConnection ofType:
                    return ofType;
                case RetryDbConnection retry:
                    c = retry.Inner;
                    break;
                case ProfiledDbConnection profiled:
                    c = profiled.WrappedConnection;
                    break;
                default:
                    throw new NotSupportedException(connection?.GetType().FullName);
            }
        }
    }

    /// <summary>
    ///     Returns the underlying transaction as a typed transaction - this is used to unwrap the profiled mini profiler stuff
    /// </summary>
    /// <typeparam name="TTransaction"></typeparam>
    /// <param name="transaction">The database transaction to unwrap.</param>
    /// <returns>The underlying typed transaction.</returns>
    public static TTransaction GetTypedTransaction<TTransaction>(IDbTransaction? transaction)
        where TTransaction : class, IDbTransaction
    {
        IDbTransaction? t = transaction;
        for (; ;)
        {
            switch (t)
            {
                case TTransaction ofType:
                    return ofType;
                case ProfiledDbTransaction profiled:
                    t = profiled.WrappedTransaction;
                    break;
                default:
                    throw new NotSupportedException(transaction?.GetType().FullName);
            }
        }
    }

    /// <summary>
    ///     Returns the underlying command as a typed command - this is used to unwrap the profiled mini profiler stuff
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <param name="command">The database command to unwrap.</param>
    /// <returns>The underlying typed command.</returns>
    public static TCommand GetTypedCommand<TCommand>(IDbCommand command)
        where TCommand : class, IDbCommand
    {
        IDbCommand? c = command;
        for (; ;)
        {
            switch (c)
            {
                case TCommand ofType:
                    return ofType;
                case FaultHandlingDbCommand faultHandling:
                    c = faultHandling.Inner;
                    break;
                case ProfiledDbCommand profiled:
                    c = profiled.InternalCommand;
                    break;
                default:
                    throw new NotSupportedException(command.GetType().FullName);
            }
        }
    }

    /// <summary>
    /// Removes all rows from the specified table in the database by executing a TRUNCATE TABLE command.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> instance on which to execute the truncate operation.</param>
    /// <param name="sqlSyntax">The <see cref="ISqlSyntaxProvider"/> used to generate the appropriate SQL syntax for truncating the table.</param>
    /// <param name="tableName">The name of the table to truncate. This should be the unquoted table name; quoting is handled internally.</param>
    /// <remarks>
    /// This operation deletes all data from the table but does not remove the table structure itself.
    /// </remarks>
    public static void TruncateTable(this IDatabase db, ISqlSyntaxProvider sqlSyntax, string tableName)
    {
        var sql = new Sql(string.Format(
            sqlSyntax.TruncateTable,
            sqlSyntax.GetQuotedTableName(tableName)));
        db.Execute(sql);
    }

    /// <summary>
    /// Gets the current transaction isolation level of the specified database.
    /// </summary>
    /// <param name="database">The database instance to retrieve the transaction isolation level from.</param>
    /// <returns>The current <see cref="System.Data.IsolationLevel"/> of the active transaction, or <see cref="System.Data.IsolationLevel.Unspecified"/> if no transaction is active.</returns>
    public static IsolationLevel GetCurrentTransactionIsolationLevel(this IDatabase database)
    {
        DbTransaction? transaction = database.Transaction;
        return transaction?.IsolationLevel ?? IsolationLevel.Unspecified;
    }

    /// <summary>
    /// Fetches results from the database by splitting the source collection into groups of a specified size, and executing a SQL query for each group.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned from the database for each group.</typeparam>
    /// <typeparam name="TSource">The type of the items in the source collection.</typeparam>
    /// <param name="db">The database instance used to perform the fetch operations.</param>
    /// <param name="source">The collection of source items to be grouped and used in queries.</param>
    /// <param name="groupSize">The maximum number of items in each group when splitting the source collection.</param>
    /// <param name="sqlFactory">A function that generates a SQL query for each group of source items.</param>
    /// <returns>An enumerable containing the results fetched from the database for each group, concatenated into a single sequence.</returns>
    public static IEnumerable<TResult> FetchByGroups<TResult, TSource>(this IDatabase db, IEnumerable<TSource> source, int groupSize, Func<IEnumerable<TSource>, Sql<ISqlContext>> sqlFactory) =>
        source.SelectByGroups(x => db.Fetch<TResult>(sqlFactory(x)), groupSize);
}
