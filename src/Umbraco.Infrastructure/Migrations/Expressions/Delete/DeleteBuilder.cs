using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Column;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Constraint;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.DefaultConstraint;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Index;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.KeysAndIndexes;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete;

public class DeleteBuilder : IDeleteBuilder
{
    private readonly IMigrationContext _context;

    public DeleteBuilder(IMigrationContext context) => _context = context;

    /// <inheritdoc />
    public IExecutableBuilder Table(string tableName)
    {
        var expression = new DeleteTableExpression(_context) { TableName = tableName };
        return new ExecutableBuilder(expression);
    }

    /// <inheritdoc />
    public IExecutableBuilder KeysAndIndexes<TDto>(bool local = true, bool foreign = true)
    {
        ISqlSyntaxProvider syntax = _context.SqlContext.SqlSyntax;
        TableDefinition tableDefinition = DefinitionFactory.GetTableDefinition(typeof(TDto), syntax);
        return KeysAndIndexes(tableDefinition.Name, local, foreign);
    }

    /// <inheritdoc />
    public IExecutableBuilder KeysAndIndexes(string? tableName, bool local = true, bool foreign = true)
    {
        if (tableName == null)
        {
            throw new ArgumentNullException(nameof(tableName));
        }

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(tableName));
        }

        return new DeleteKeysAndIndexesBuilder(_context)
        {
            TableName = tableName,
            DeleteLocal = local,
            DeleteForeign = foreign,
        };
    }

    /// <inheritdoc />
    public IDeleteColumnBuilder Column(string columnName)
    {
        var expression = new DeleteColumnExpression(_context) { ColumnNames = { columnName } };
        return new DeleteColumnBuilder(expression);
    }

    /// <inheritdoc />
    public IDeleteForeignKeyFromTableBuilder ForeignKey()
    {
        var expression = new DeleteForeignKeyExpression(_context);
        return new DeleteForeignKeyBuilder(expression);
    }

    /// <inheritdoc />
    public IDeleteForeignKeyOnTableBuilder ForeignKey(string foreignKeyName)
    {
        var expression = new DeleteForeignKeyExpression(_context) { ForeignKey = { Name = foreignKeyName } };
        return new DeleteForeignKeyBuilder(expression);
    }

    /// <inheritdoc />
    public IDeleteDataBuilder FromTable(string tableName)
    {
        var expression = new DeleteDataExpression(_context) { TableName = tableName };
        return new DeleteDataBuilder(expression);
    }

    /// <inheritdoc />
    public IDeleteIndexForTableBuilder Index()
    {
        var expression = new DeleteIndexExpression(_context);
        return new DeleteIndexBuilder(expression);
    }

    /// <inheritdoc />
    public IDeleteIndexForTableBuilder Index(string indexName)
    {
        var expression = new DeleteIndexExpression(_context) { Index = { Name = indexName } };
        return new DeleteIndexBuilder(expression);
    }

    /// <inheritdoc />
    public IDeleteConstraintBuilder PrimaryKey(string primaryKeyName)
    {
        var expression = new DeleteConstraintExpression(_context, ConstraintType.PrimaryKey)
        {
            Constraint = { ConstraintName = primaryKeyName },
        };
        return new DeleteConstraintBuilder(expression);
    }

    /// <inheritdoc />
    public IDeleteConstraintBuilder UniqueConstraint(string constraintName)
    {
        var expression = new DeleteConstraintExpression(_context, ConstraintType.Unique)
        {
            Constraint = { ConstraintName = constraintName },
        };
        return new DeleteConstraintBuilder(expression);
    }

    /// <inheritdoc />
    public IDeleteDefaultConstraintOnTableBuilder DefaultConstraint()
    {
        var expression = new DeleteDefaultConstraintExpression(_context);
        return new DeleteDefaultConstraintBuilder(_context, expression);
    }
}
