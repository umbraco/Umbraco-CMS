using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using NPoco;
using NPoco.SqlServer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to NPoco Database class.
/// </summary>
public static partial class NPocoDatabaseExtensions
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
        SqlBulkCopyHelper.SqlConnectionResolver = dbConn => GetTypedConnection<SqlConnection>(dbConn);
        SqlBulkCopyHelper.SqlTransactionResolver = dbTran => GetTypedTransaction<SqlTransaction>(dbTran);
    }

    /// <summary>
    ///     Determines whether a column should be part of a bulk-insert.
    /// </summary>
    /// <param name="pocoData">The PocoData object corresponding to the record's type.</param>
    /// <param name="column">The column.</param>
    /// <returns>A value indicating whether the column should be part of the bulk-insert.</returns>
    /// <remarks>Columns that are primary keys and auto-incremental, or result columns, are excluded from bulk-inserts.</remarks>
    public static bool IncludeColumn(PocoData pocoData, KeyValuePair<string, PocoColumn> column) =>
        column.Value.ResultColumn == false
        && (pocoData.TableInfo.AutoIncrement == false || column.Key != pocoData.TableInfo.PrimaryKey);

    /// <summary>
    ///     Creates bulk-insert commands.
    /// </summary>
    /// <typeparam name="T">The type of the records.</typeparam>
    /// <param name="database">The database.</param>
    /// <param name="records">The records.</param>
    /// <returns>The sql commands to execute.</returns>
    internal static IDbCommand[] GenerateBulkInsertCommands<T>(this IUmbracoDatabase database, T[] records)
    {
        if (database.Connection == null)
        {
            throw new ArgumentException("Null database?.connection.", nameof(database));
        }

        PocoData pocoData = database.PocoDataFactory.ForType(typeof(T));

        // get columns to include, = number of parameters per row
        KeyValuePair<string, PocoColumn>[] columns =
            pocoData.Columns.Where(c => IncludeColumn(pocoData, c)).ToArray();
        var paramsPerRecord = columns.Length;

        // format columns to sql
        var tableName = database.DatabaseType.EscapeTableName(pocoData.TableInfo.TableName);
        var columnNames = string.Join(
            ", ",
            columns.Select(c => tableName + "." + database.DatabaseType.EscapeSqlIdentifier(c.Key)));

        // example:
        // assume 4168 records, each record containing 8 fields, ie 8 command parameters
        // max 2100 parameter per command
        // Math.Floor(2100 / 8) = 262 record per command
        // 4168 / 262 = 15.908... = there will be 16 command in total
        // (if we have disabled db parameters, then all records will be included, in only one command)
        var recordsPerCommand = paramsPerRecord == 0
            ? int.MaxValue
            : Convert.ToInt32(Math.Floor((double)Constants.Sql.MaxParameterCount / paramsPerRecord));
        var commandsCount = Convert.ToInt32(Math.Ceiling((double)records.Length / recordsPerCommand));

        var commands = new IDbCommand[commandsCount];
        var recordsIndex = 0;
        var recordsLeftToInsert = records.Length;
        var prefix = database.DatabaseType.GetParameterPrefix(database.ConnectionString);
        for (var commandIndex = 0; commandIndex < commandsCount; commandIndex++)
        {
            DbCommand command = database.CreateCommand(database.Connection, CommandType.Text, string.Empty);
            var parameterIndex = 0;
            var commandRecords = Math.Min(recordsPerCommand, recordsLeftToInsert);
            var recordsValues = new string[commandRecords];
            for (var commandRecordIndex = 0;
                 commandRecordIndex < commandRecords;
                 commandRecordIndex++, recordsIndex++, recordsLeftToInsert--)
            {
                T record = records[recordsIndex];
                var recordValues = new string[columns.Length];
                for (var columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                {
                    database.AddParameter(command, columns[columnIndex].Value.GetValue(record));
                    recordValues[columnIndex] = prefix + parameterIndex++;
                }

                recordsValues[commandRecordIndex] = "(" + string.Join(",", recordValues) + ")";
            }

            command.CommandText =
                $"INSERT INTO {tableName} ({columnNames}) VALUES {string.Join(", ", recordsValues)}";
            commands[commandIndex] = command;
        }

        return commands;
    }
}
