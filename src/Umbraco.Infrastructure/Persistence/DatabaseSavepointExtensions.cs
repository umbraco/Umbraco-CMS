using System.Data.Common;
using NPoco;
using StackExchange.Profiling.Data;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for running database operations whose failure the caller intends to handle.
/// </summary>
public static class DatabaseSavepointExtensions
{
    private static long _savepointCounter;

    /// <summary>
    ///     Runs an operation inside a savepoint of the current transaction, so that a database error it raises can be
    ///     handled by the caller without leaving the transaction unusable.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Some databases abort the whole transaction after any failed statement, so code that catches a database
    ///         error and carries on in the same transaction must isolate the statement that may fail. When the operation
    ///         throws, the transaction is rolled back to the savepoint and the exception is rethrown.
    ///     </para>
    ///     <para>
    ///         When there is no transaction, or the transaction does not support savepoints, the operation runs as is.
    ///     </para>
    /// </remarks>
    /// <param name="database">The database.</param>
    /// <param name="operation">The operation that may fail.</param>
    /// <typeparam name="T">The type of the operation result.</typeparam>
    /// <returns>The result of the operation.</returns>
    public static T ExecuteInSavepoint<T>(this IDatabase database, Func<T> operation)
    {
        DbTransaction? transaction = GetSavepointTransaction(database);
        if (transaction is null || transaction.SupportsSavepoints is false)
        {
            return operation();
        }

        var savepoint = NextSavepointName();
        transaction.Save(savepoint);
        try
        {
            T result = operation();
            transaction.Release(savepoint);
            return result;
        }
        catch
        {
            transaction.Rollback(savepoint);
            throw;
        }
    }

    /// <inheritdoc cref="ExecuteInSavepoint{T}(IDatabase, Func{T})" />
    public static void ExecuteInSavepoint(this IDatabase database, Action operation)
        => database.ExecuteInSavepoint(() =>
        {
            operation();
            return true;
        });

    /// <inheritdoc cref="ExecuteInSavepoint{T}(IDatabase, Func{T})" />
    public static async Task<T> ExecuteInSavepointAsync<T>(this IDatabase database, Func<Task<T>> operation)
    {
        DbTransaction? transaction = GetSavepointTransaction(database);
        if (transaction is null || transaction.SupportsSavepoints is false)
        {
            return await operation();
        }

        var savepoint = NextSavepointName();
        await transaction.SaveAsync(savepoint);
        try
        {
            T result = await operation();
            await transaction.ReleaseAsync(savepoint);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(savepoint);
            throw;
        }
    }

    // A profiled transaction does not forward savepoint support, so savepoints are taken on the transaction it wraps.
    private static DbTransaction? GetSavepointTransaction(IDatabase database)
    {
        DbTransaction? transaction = database.Transaction;
        while (transaction is ProfiledDbTransaction profiled)
        {
            transaction = profiled.WrappedTransaction;
        }

        return transaction;
    }

    private static string NextSavepointName() => $"umb_sp_{Interlocked.Increment(ref _savepointCounter)}";
}
