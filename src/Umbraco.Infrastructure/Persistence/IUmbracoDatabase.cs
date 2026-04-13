using System.Data.Common;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Defines an abstraction for interacting with the Umbraco database, providing methods for persistence and data access operations.
/// </summary>
public interface IUmbracoDatabase : IDatabase
{
    /// <summary>
    ///     Gets the Sql context.
    /// </summary>
    ISqlContext SqlContext { get; }

    /// <summary>
    ///     Gets the database instance unique identifier as a string.
    /// </summary>
    /// <remarks>
    ///     UmbracoDatabase returns the first eight digits of its unique Guid and, in some
    ///     debug mode, the underlying database connection identifier (if any).
    /// </remarks>
    string InstanceId { get; }

    /// <summary>
    ///     Gets a value indicating whether the database is currently in a transaction.
    /// </summary>
    bool InTransaction { get; }

    /// <summary>
    /// Gets or sets a value indicating whether SQL count queries are enabled for this database instance.
    /// When enabled, the database will track and expose the number of executed SQL queries.
    /// </summary>
    bool EnableSqlCount { get; set; }

    /// <summary>
    /// Gets the total number of SQL queries that have been executed by this database instance.
    /// </summary>
    int SqlCount { get; }

    /// <summary>
    /// Inserts a collection of records into the database in a single bulk operation.
    /// </summary>
    /// <typeparam name="T">The type of the records to insert.</typeparam>
    /// <param name="records">The collection of records to insert.</param>
    /// <returns>The number of records successfully inserted.</returns>
    int BulkInsertRecords<T>(IEnumerable<T> records);

    /// <summary>
    /// Gets a value indicating whether Umbraco is installed.
    /// </summary>
    /// <returns>True if Umbraco is installed; otherwise, false.</returns>
    bool IsUmbracoInstalled();

    /// <summary>
    /// Checks the current database schema against the expected schema for Umbraco, identifying any discrepancies or issues.
    /// </summary>
    /// <returns>A <see cref="DatabaseSchemaResult"/> containing details about the validation outcome, including any missing or invalid schema elements.</returns>
    DatabaseSchemaResult ValidateSchema();

    /// <summary>
    /// Executes a non-query command (such as INSERT, UPDATE, or DELETE) against the database.
    /// </summary>
    /// <param name="command">The <see cref="DbCommand"/> to execute.</param>
    /// <returns>The number of rows affected by the command.</returns>
    int ExecuteNonQuery(DbCommand command) => command.ExecuteNonQuery();
}
