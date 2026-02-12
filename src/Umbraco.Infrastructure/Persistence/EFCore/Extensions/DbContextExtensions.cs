using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Extensions;

/// <summary>
/// Provides extension methods for Entity Framework Core DbContext operations.
/// </summary>
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

    /// <summary>
    /// Migrates the database to a specific migration identified by its type.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="targetMigration">The type of the target migration.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the type does not have a MigrationAttribute.</exception>
    public static async Task MigrateDatabaseAsync(this DbContext context, Type targetMigration)
    {
        MigrationAttribute? migrationAttribute = targetMigration.GetCustomAttribute<MigrationAttribute>(false);

        if (migrationAttribute is null)
        {
            throw new ArgumentException("The type does not have a MigrationAttribute", nameof(targetMigration));
        }

        await context.MigrateDatabaseAsync(migrationAttribute.Id);
    }

    /// <summary>
    /// Migrates the database to a specific migration identified by its ID.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="targetMigrationId">The ID of the target migration.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task MigrateDatabaseAsync(this DbContext context, string targetMigrationId)
    {
        await context.GetService<IMigrator>().MigrateAsync(targetMigrationId);
    }
}
