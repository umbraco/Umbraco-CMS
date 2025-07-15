using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Provides a base class to all migrations.
/// </summary>
public abstract partial class AsyncMigrationBase
{
    // provides extra methods for migrations
    protected void AddColumn<T>(string columnName)
    {
        TableDefinition? table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        AddColumn(table, table.Name!, columnName);
    }

    protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string columnName)
    {
        TableDefinition? table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        if (columns.Any(x => x.TableName.InvariantEquals(table.Name) && !x.ColumnName.InvariantEquals(columnName)))
        {
            AddColumn(table, table.Name!, columnName);
        }
    }

    protected void AddColumn<T>(string tableName, string columnName)
    {
        TableDefinition? table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        AddColumn(table, tableName, columnName);
    }

    protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName)
    {
        TableDefinition? table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        if (columns.Any(x => x.TableName.InvariantEquals(tableName) && !x.ColumnName.InvariantEquals(columnName)))
        {
            AddColumn(table, tableName, columnName);
        }
    }

    protected void AddColumn<T>(string columnName, out IEnumerable<string> sqls)
    {
        TableDefinition? table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        AddColumn(table, table.Name!, columnName, out sqls);
    }

    private void AddColumn(TableDefinition table, string tableName, string columnName)
    {
        if (ColumnExists(tableName, columnName))
        {
            return;
        }

        ColumnDefinition? column = table.Columns.First(x => x.Name == columnName);
        var createSql = SqlSyntax.Format(column);

        Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();
    }

    protected void AddColumn<T>(string tableName, string columnName, out IEnumerable<string> sqls)
    {
        TableDefinition? table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        AddColumn(table, tableName, columnName, out sqls);
    }

    protected void AlterColumn<T>(string tableName, string columnName)
    {
        TableDefinition? table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        ColumnDefinition? column = table.Columns.First(x => x.Name == columnName);
        SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out IEnumerable<string>? sqls);
        foreach (var sql in sqls)
        {
            Execute.Sql(sql).Do();
        }
    }

    private void AddColumn(TableDefinition table, string tableName, string columnName, out IEnumerable<string> sqls)
    {
        if (ColumnExists(tableName, columnName))
        {
            sqls = Enumerable.Empty<string>();
            return;
        }

        ColumnDefinition? column = table.Columns.First(x => x.Name == columnName);
        var createSql = SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out sqls);
        Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();
    }

    protected void ReplaceColumn<T>(string tableName, string currentName, string newName)
    {
        Execute.Sql(SqlSyntax.FormatColumnRename(tableName, currentName, newName)).Do();
        AlterColumn<T>(tableName, newName);
    }

    protected bool TableExists(string tableName)
    {
        IEnumerable<string>? tables = SqlSyntax.GetTablesInSchema(Context.Database);
        return tables.Any(x => x.InvariantEquals(tableName));
    }

    protected bool IndexExists(string indexName)
    {
        IEnumerable<Tuple<string, string, string, bool>>? indexes = SqlSyntax.GetDefinedIndexes(Context.Database);
        return indexes.Any(x => x.Item2.InvariantEquals(indexName));
    }

    protected void CreateIndex<T>(string toCreate)
    {
        TableDefinition tableDef = DefinitionFactory.GetTableDefinition(typeof(T), Context.SqlContext.SqlSyntax);
        IndexDefinition index = tableDef.Indexes.First(x => x.Name == toCreate);
        new ExecuteSqlStatementExpression(Context) { SqlStatement = Context.SqlContext.SqlSyntax.Format(index) }
            .Execute();
    }

    protected void DeleteIndex<T>(string toDelete)
    {
        if (!IndexExists(toDelete))
        {
            return;
        }

        TableDefinition tableDef = DefinitionFactory.GetTableDefinition(typeof(T), Context.SqlContext.SqlSyntax);
        Delete.Index(toDelete).OnTable(tableDef.Name).Do();
    }

    protected bool PrimaryKeyExists(string tableName, string primaryKeyName)
    {
        return SqlSyntax.DoesPrimaryKeyExist(Context.Database, tableName, primaryKeyName);
    }

    protected bool ColumnExists(string tableName, string columnName)
    {
        ColumnInfo[]? columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
        return columns.Any(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
    }

    protected string? ColumnType(string tableName, string columnName)
    {
        ColumnInfo[]? columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
        ColumnInfo? column = columns.FirstOrDefault(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
        return column?.DataType;
    }
}
