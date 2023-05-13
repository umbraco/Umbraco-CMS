using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Umbraco.Extensions;

public static class DbContextExtensions
{
    /// <summary>
    /// Executes a raw SQL query and returns the result.
    /// </summary>
    /// <param name="database">The database.</param>
    /// <param name="sql">The sql query.</param>
    /// <param name="parameters">The list of db parameters.</param>
    /// <param name="commandType">The command type.</param>
    /// <param name="commandTimeOut">The amount of time to wait before the command times out.</param>
    /// <typeparam name="T">the type to return.</typeparam>
    /// <returns>Returns an object of the given type T.</returns>
    public static async Task<T?> ExecuteScalarAsync<T>(this DatabaseFacade database, string sql, List<DbParameter>? parameters = null, CommandType commandType = CommandType.Text, TimeSpan? commandTimeOut = null)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(sql);

        await using DbCommand dbCommand = database.GetDbConnection().CreateCommand();

        if (database.CurrentTransaction is not null)
        {
            dbCommand.Transaction = database.CurrentTransaction.GetDbTransaction();
        }

        if (dbCommand.Connection?.State != ConnectionState.Open)
        {
            await dbCommand.Connection!.OpenAsync();
        }

        dbCommand.CommandText = sql;
        dbCommand.CommandType = commandType;
        if (commandTimeOut is not null)
        {
            dbCommand.CommandTimeout = (int)commandTimeOut.Value.TotalSeconds;
        }

        if (parameters != null)
        {
            dbCommand.Parameters.AddRange(parameters.ToArray());
        }

        var result = await dbCommand.ExecuteScalarAsync();
        return (T?)result;
    }
}
