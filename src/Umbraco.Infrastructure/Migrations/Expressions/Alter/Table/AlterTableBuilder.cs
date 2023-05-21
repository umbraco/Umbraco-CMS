using System.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table;

public class AlterTableBuilder : ExpressionBuilderBase<AlterTableExpression, IAlterTableColumnOptionBuilder>,
    IAlterTableColumnTypeBuilder,
    IAlterTableColumnOptionForeignKeyCascadeBuilder
{
    private readonly IMigrationContext _context;

    public AlterTableBuilder(IMigrationContext context, AlterTableExpression expression)
        : base(expression) =>
        _context = context;

    public ColumnDefinition CurrentColumn { get; set; } = null!;

    public ForeignKeyDefinition CurrentForeignKey { get; set; } = null!;

    public void Do()
    {
        if (_context.Database.DatabaseType.IsSqlite())
        {
            throw new NotSupportedException($"SQLite does not support ALTER TABLE operations. Instead you will have to:{Environment.NewLine}1. Create a temp table.{Environment.NewLine}2. Copy data from existing table into the temp table.{Environment.NewLine}3. Delete the existing table.{Environment.NewLine}4. Create a new table with the name of the table you're trying to alter, but with a new signature{Environment.NewLine}5. Copy data from the temp table into the new table.{Environment.NewLine}6. Delete the temp table.");
        }
        Expression.Execute();
    }

    public IAlterTableColumnOptionBuilder WithDefault(SystemMethods method)
    {
        CurrentColumn.DefaultValue = method;
        return this;
    }

    public IAlterTableColumnOptionBuilder WithDefaultValue(object value)
    {
        if (CurrentColumn.ModificationType == ModificationType.Alter)
        {
            var dc = new AlterDefaultConstraintExpression(_context)
            {
                TableName = Expression.TableName,
                ColumnName = CurrentColumn.Name,
                DefaultValue = value,
            };

            Expression.Expressions.Add(dc);
        }

        CurrentColumn.DefaultValue = value;
        return this;
    }

    public IAlterTableColumnOptionBuilder Identity()
    {
        CurrentColumn.IsIdentity = true;
        return this;
    }

    public IAlterTableColumnOptionBuilder Indexed() => Indexed(null);

    public IAlterTableColumnOptionBuilder Indexed(string? indexName)
    {
        CurrentColumn.IsIndexed = true;

        var index = new CreateIndexExpression(
            _context,
            new IndexDefinition { Name = indexName, TableName = Expression.TableName });

        index.Index.Columns.Add(new IndexColumnDefinition { Name = CurrentColumn.Name });

        Expression.Expressions.Add(index);

        return this;
    }

    public IAlterTableColumnOptionBuilder PrimaryKey()
    {
        CurrentColumn.IsPrimaryKey = true;

        var expression = new CreateConstraintExpression(_context, ConstraintType.PrimaryKey)
        {
            Constraint = { TableName = Expression.TableName, Columns = new[] { CurrentColumn.Name } },
        };
        Expression.Expressions.Add(expression);

        return this;
    }

    public IAlterTableColumnOptionBuilder PrimaryKey(string primaryKeyName)
    {
        CurrentColumn.IsPrimaryKey = true;
        CurrentColumn.PrimaryKeyName = primaryKeyName;

        var expression = new CreateConstraintExpression(_context, ConstraintType.PrimaryKey)
        {
            Constraint =
            {
                ConstraintName = primaryKeyName,
                TableName = Expression.TableName,
                Columns = new[] { CurrentColumn.Name }
            },
        };
        Expression.Expressions.Add(expression);

        return this;
    }

    public IAlterTableColumnOptionBuilder Nullable()
    {
        CurrentColumn.IsNullable = true;
        return this;
    }

    public IAlterTableColumnOptionBuilder NotNullable()
    {
        CurrentColumn.IsNullable = false;
        return this;
    }

    public IAlterTableColumnOptionBuilder Unique() => Unique(null);

    public IAlterTableColumnOptionBuilder Unique(string? indexName)
    {
        CurrentColumn.IsUnique = true;

        var index = new CreateIndexExpression(
            _context,
            new IndexDefinition
            {
                Name = indexName,
                TableName = Expression.TableName,
                IndexType = IndexTypes.UniqueNonClustered,
            });

        index.Index.Columns.Add(new IndexColumnDefinition { Name = CurrentColumn.Name });

        Expression.Expressions.Add(index);

        return this;
    }

    public IAlterTableColumnOptionForeignKeyCascadeBuilder
        ForeignKey(string primaryTableName, string primaryColumnName) =>
        ForeignKey(null, null, primaryTableName, primaryColumnName);

    public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey(string foreignKeyName, string primaryTableName,
        string primaryColumnName) =>
        ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);

    public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey(
        string? foreignKeyName,
        string? primaryTableSchema,
        string primaryTableName, string primaryColumnName)
    {
        CurrentColumn.IsForeignKey = true;

        var fk = new CreateForeignKeyExpression(
            _context,
            new ForeignKeyDefinition
            {
                Name = foreignKeyName,
                PrimaryTable = primaryTableName,
                PrimaryTableSchema = primaryTableSchema,
                ForeignTable = Expression.TableName,
            });

        fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);
        fk.ForeignKey.ForeignColumns.Add(CurrentColumn.Name);

        Expression.Expressions.Add(fk);
        CurrentForeignKey = fk.ForeignKey;
        return this;
    }

    public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey()
    {
        CurrentColumn.IsForeignKey = true;
        return this;
    }

    public IAlterTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(
        string foreignTableName,
        string foreignColumnName) => ReferencedBy(null, null, foreignTableName, foreignColumnName);

    public IAlterTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(string foreignKeyName, string foreignTableName,
        string foreignColumnName) =>
        ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);

    public IAlterTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(
        string? foreignKeyName,
        string? foreignTableSchema,
        string foreignTableName, string foreignColumnName)
    {
        var fk = new CreateForeignKeyExpression(
            _context,
            new ForeignKeyDefinition
            {
                Name = foreignKeyName,
                PrimaryTable = Expression.TableName,
                ForeignTable = foreignTableName,
                ForeignTableSchema = foreignTableSchema,
            });

        fk.ForeignKey.PrimaryColumns.Add(CurrentColumn.Name);
        fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

        Expression.Expressions.Add(fk);
        CurrentForeignKey = fk.ForeignKey;
        return this;
    }

    public IAlterTableColumnTypeBuilder AddColumn(string name)
    {
        var column = new ColumnDefinition { Name = name, ModificationType = ModificationType.Create };
        var createColumn = new CreateColumnExpression(_context) { Column = column, TableName = Expression.TableName };

        CurrentColumn = column;

        Expression.Expressions.Add(createColumn);
        return this;
    }

    public IAlterTableColumnTypeBuilder AlterColumn(string name)
    {
        var column = new ColumnDefinition { Name = name, ModificationType = ModificationType.Alter };
        var alterColumn = new AlterColumnExpression(_context) { Column = column, TableName = Expression.TableName };

        CurrentColumn = column;

        Expression.Expressions.Add(alterColumn);
        return this;
    }

    public IAlterTableColumnOptionForeignKeyCascadeBuilder OnDelete(Rule rule)
    {
        CurrentForeignKey.OnDelete = rule;
        return this;
    }

    public IAlterTableColumnOptionForeignKeyCascadeBuilder OnUpdate(Rule rule)
    {
        CurrentForeignKey.OnUpdate = rule;
        return this;
    }

    public IAlterTableColumnOptionBuilder OnDeleteOrUpdate(Rule rule)
    {
        OnDelete(rule);
        OnUpdate(rule);
        return this;
    }

    public override ColumnDefinition GetColumnForType() => CurrentColumn;
}
