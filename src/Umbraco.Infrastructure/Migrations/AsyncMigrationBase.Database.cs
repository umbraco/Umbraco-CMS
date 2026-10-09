using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Provides a base class to all migrations.
/// </summary>
public abstract partial class AsyncMigrationBase
{
    /// <summary>
    /// Ensures that long-running database operations in this migration are allowed at least five minutes.
    /// </summary>
    /// <remarks>
    /// A command timeout that is already at least as long is left alone, whether it was set on the database
    /// or derived by the provider from the connection string, and including a timeout of no limit at all.
    /// </remarks>
    /// <param name="database">The database instance for which the command timeout is being ensured.</param>
    protected static void EnsureLongCommandTimeout(NPoco.IDatabase database)
    {
        const int MinimumCommandTimeoutInSeconds = 300;

        int? effectiveCommandTimeout;
        if (database is UmbracoDatabase umbracoDatabase)
        {
            effectiveCommandTimeout = umbracoDatabase.EffectiveCommandTimeout;
        }
        else
        {
            effectiveCommandTimeout = database.CommandTimeout > 0 ? database.CommandTimeout : null;
        }

        if (effectiveCommandTimeout is 0 || effectiveCommandTimeout >= MinimumCommandTimeoutInSeconds)
        {
            return;
        }

        database.CommandTimeout = MinimumCommandTimeoutInSeconds;
    }

    /// <summary>
    /// Adds the column to the table defined by <typeparamref name="T"/>, unless it already exists.
    /// </summary>
    /// <typeparam name="T">The model type that defines the table and the column.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <returns><c>true</c> if the column was added; otherwise, <c>false</c>.</returns>
    protected bool AddColumn<T>(string columnName)
        => TryAddColumn<T>(columnName, tableName: null, columns: null);

    /// <summary>
    /// Adds the column to the specified table, unless it already exists.
    /// </summary>
    /// <typeparam name="T">The model type that defines the column.</typeparam>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns><c>true</c> if the column was added; otherwise, <c>false</c>.</returns>
    protected bool AddColumn<T>(string tableName, string columnName)
        => TryAddColumn<T>(columnName, tableName, columns: null);

    /// <summary>
    /// Adds the column to the table defined by <typeparamref name="T"/>, unless it already exists in the specified column information.
    /// </summary>
    /// <typeparam name="T">The model type that defines the table and the column.</typeparam>
    /// <param name="columns">The column information of the current schema, as returned by <see cref="ISqlSyntaxProvider.GetColumnsInSchema" />.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns><c>true</c> if the column was added; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Use this overload to add multiple columns without querying the schema for each of them.
    /// </remarks>
    protected bool AddColumn<T>(IEnumerable<ColumnInfo> columns, string columnName)
        => TryAddColumn<T>(columnName, tableName: null, columns);

    /// <summary>
    /// Adds the column to the specified table, unless it already exists in the specified column information.
    /// </summary>
    /// <typeparam name="T">The model type that defines the column.</typeparam>
    /// <param name="columns">The column information of the current schema, as returned by <see cref="ISqlSyntaxProvider.GetColumnsInSchema" />.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns><c>true</c> if the column was added; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Use this overload to add multiple columns without querying the schema for each of them.
    /// </remarks>
    protected bool AddColumn<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName)
        => TryAddColumn<T>(columnName, tableName, columns);

    /// <summary>
    /// Adds the column to the table defined by <typeparamref name="T"/>, unless it already exists in the specified column information.
    /// </summary>
    /// <typeparam name="T">The model type that defines the table and the column.</typeparam>
    /// <param name="columns">The column information of the current schema, as returned by <see cref="ISqlSyntaxProvider.GetColumnsInSchema" />.</param>
    /// <param name="columnName">The name of the column.</param>
    [Obsolete("Use AddColumn<T>(columns, columnName) instead, which also only adds the column if it does not exist. Scheduled for removal in Umbraco 21.")]
    protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string columnName)
        => AddColumn<T>(columns, columnName);

    /// <summary>
    /// Adds the column to the specified table, unless it already exists in the specified column information.
    /// </summary>
    /// <typeparam name="T">The model type that defines the column.</typeparam>
    /// <param name="columns">The column information of the current schema, as returned by <see cref="ISqlSyntaxProvider.GetColumnsInSchema" />.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    [Obsolete("Use AddColumn<T>(columns, tableName, columnName) instead, which also only adds the column if it does not exist. Scheduled for removal in Umbraco 21.")]
    protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName)
        => AddColumn<T>(columns, tableName, columnName);

    /// <summary>
    /// Adds the column to the table defined by <typeparamref name="T"/> as nullable, unless it already exists,
    /// and returns the SQL statements that apply the remaining column definition (such as making it non-nullable).
    /// </summary>
    /// <typeparam name="T">The model type that defines the table and the column.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="sqls">The SQL statements to execute after the existing rows have been given a value for the new column; empty if the column already existed.</param>
    /// <returns><c>true</c> if the column was added; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Use this overload to add a non-nullable column without a default value to a table that already contains rows:
    /// add the column, update the existing rows and then execute the returned SQL statements.
    /// </remarks>
    protected bool AddColumn<T>(string columnName, out IEnumerable<string> sqls)
        => TryAddColumn<T>(columnName, tableName: null, columns: null, out sqls);

    /// <summary>
    /// Adds the column to the specified table as nullable, unless it already exists,
    /// and returns the SQL statements that apply the remaining column definition (such as making it non-nullable).
    /// </summary>
    /// <typeparam name="T">The model type that defines the column.</typeparam>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="sqls">The SQL statements to execute after the existing rows have been given a value for the new column; empty if the column already existed.</param>
    /// <returns><c>true</c> if the column was added; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Use this overload to add a non-nullable column without a default value to a table that already contains rows:
    /// add the column, update the existing rows and then execute the returned SQL statements.
    /// </remarks>
    protected bool AddColumn<T>(string tableName, string columnName, out IEnumerable<string> sqls)
        => TryAddColumn<T>(columnName, tableName, columns: null, out sqls);

    /// <summary>
    /// Alters the column of the table defined by <typeparamref name="T"/> to match its column definition.
    /// </summary>
    /// <typeparam name="T">The model type that defines the table and the column.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    protected void AlterColumn<T>(string columnName)
        => AlterColumn<T>(tableName: null, columnName);

    /// <summary>
    /// Alters the column of the specified table to match its column definition.
    /// </summary>
    /// <typeparam name="T">The model type that defines the column.</typeparam>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    protected void AlterColumn<T>(string? tableName, string columnName)
    {
        TableDefinition table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        tableName ??= table.Name;

        ColumnDefinition column = GetColumnDefinition(table, columnName);
        SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out IEnumerable<string> sqls);
        foreach (var sql in sqls)
        {
            Execute.Sql(sql).Do();
        }
    }

    /// <summary>
    /// Renames the column of the table defined by <typeparamref name="T"/> and alters it to match its column definition.
    /// </summary>
    /// <typeparam name="T">The model type that defines the table and the column.</typeparam>
    /// <param name="currentName">The current name of the column.</param>
    /// <param name="newName">The new name of the column.</param>
    protected void ReplaceColumn<T>(string currentName, string newName)
        => ReplaceColumn<T>(tableName: null, currentName, newName);

    /// <summary>
    /// Renames the column of the specified table and alters it to match its column definition.
    /// </summary>
    /// <typeparam name="T">The model type that defines the column.</typeparam>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="currentName">The current name of the column.</param>
    /// <param name="newName">The new name of the column.</param>
    protected void ReplaceColumn<T>(string? tableName, string currentName, string newName)
    {
        tableName ??= DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax).Name;

        Execute.Sql(SqlSyntax.FormatColumnRename(tableName, currentName, newName)).Do();
        AlterColumn<T>(tableName, newName);
    }

    /// <summary>
    /// Determines whether the table exists.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns><c>true</c> if the table exists; otherwise, <c>false</c>.</returns>
    protected bool TableExists(string tableName)
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);
        return tables.Any(x => x.InvariantEquals(tableName));
    }

    /// <summary>
    /// Determines whether the index exists.
    /// </summary>
    /// <param name="indexName">The name of the index.</param>
    /// <returns><c>true</c> if the index exists; otherwise, <c>false</c>.</returns>
    protected bool IndexExists(string indexName)
    {
        IEnumerable<Tuple<string, string, string, bool>> indexes = SqlSyntax.GetDefinedIndexes(Context.Database);
        return indexes.Any(x => x.Item2.InvariantEquals(indexName));
    }

    /// <summary>
    /// Creates the index defined by <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The model type that defines the index.</typeparam>
    /// <param name="toCreate">The name of the index.</param>
    protected void CreateIndex<T>(string toCreate)
    {
        TableDefinition tableDef = DefinitionFactory.GetTableDefinition(typeof(T), Context.SqlContext.SqlSyntax);
        IndexDefinition index = tableDef.Indexes.First(x => x.Name == toCreate);
        new ExecuteSqlStatementExpression(Context) { SqlStatement = Context.SqlContext.SqlSyntax.Format(index) }
            .Execute();
    }

    /// <summary>
    /// Deletes the index from the table defined by <typeparamref name="T"/>, if it exists.
    /// </summary>
    /// <typeparam name="T">The model type that defines the table.</typeparam>
    /// <param name="toDelete">The name of the index.</param>
    protected void DeleteIndex<T>(string toDelete)
    {
        if (!IndexExists(toDelete))
        {
            return;
        }

        TableDefinition tableDef = DefinitionFactory.GetTableDefinition(typeof(T), Context.SqlContext.SqlSyntax);
        Delete.Index(toDelete).OnTable(tableDef.Name).Do();
    }

    /// <summary>
    /// Determines whether the primary key exists on the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="primaryKeyName">The name of the primary key.</param>
    /// <returns><c>true</c> if the primary key exists; otherwise, <c>false</c>.</returns>
    protected bool PrimaryKeyExists(string tableName, string primaryKeyName)
        => SqlSyntax.DoesPrimaryKeyExist(Context.Database, tableName, primaryKeyName);

    /// <summary>
    /// Determines whether the column exists in the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns><c>true</c> if the column exists; otherwise, <c>false</c>.</returns>
    protected bool ColumnExists(string tableName, string columnName)
        => ColumnExists(SqlSyntax.GetColumnsInSchema(Context.Database), tableName, columnName);

    /// <summary>
    /// Determines whether the column exists in the specified table, according to the specified column information.
    /// </summary>
    /// <param name="columns">The column information of the current schema, as returned by <see cref="ISqlSyntaxProvider.GetColumnsInSchema" />.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns><c>true</c> if the column exists; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Use this overload to check multiple columns without querying the schema for each of them.
    /// </remarks>
    protected static bool ColumnExists(IEnumerable<ColumnInfo> columns, string tableName, string columnName)
        => columns.Any(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));

    /// <summary>
    /// Gets the data type of the column in the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>The data type of the column, or <c>null</c> if the column does not exist.</returns>
    protected string? ColumnType(string tableName, string columnName)
    {
        IEnumerable<ColumnInfo> columns = SqlSyntax.GetColumnsInSchema(Context.Database);
        ColumnInfo? column = columns.FirstOrDefault(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
        return column?.DataType;
    }

    private bool TryAddColumn<T>(string columnName, string? tableName, IEnumerable<ColumnInfo>? columns)
    {
        TableDefinition table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        tableName ??= table.Name;

        if (ColumnExists(columns ?? SqlSyntax.GetColumnsInSchema(Context.Database), tableName, columnName))
        {
            return false;
        }

        ColumnDefinition column = GetColumnDefinition(table, columnName);
        var createSql = SqlSyntax.Format(column);
        Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();

        return true;
    }

    private bool TryAddColumn<T>(string columnName, string? tableName, IEnumerable<ColumnInfo>? columns, out IEnumerable<string> sqls)
    {
        TableDefinition table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        tableName ??= table.Name;

        if (ColumnExists(columns ?? SqlSyntax.GetColumnsInSchema(Context.Database), tableName, columnName))
        {
            sqls = [];
            return false;
        }

        ColumnDefinition column = GetColumnDefinition(table, columnName);
        var createSql = SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out sqls);
        Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();

        return true;
    }

    private static ColumnDefinition GetColumnDefinition(TableDefinition table, string columnName)
        => table.Columns.FirstOrDefault(x => x.Name.InvariantEquals(columnName))
            ?? throw new InvalidOperationException($"The table definition of '{table.Name}' does not define a column named '{columnName}'.");
}
