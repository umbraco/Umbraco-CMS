using System.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

public class CreateTableBuilder : ExpressionBuilderBase<CreateTableExpression, ICreateTableColumnOptionBuilder>,
    ICreateTableColumnAsTypeBuilder,
    ICreateTableColumnOptionForeignKeyCascadeBuilder
{
    private readonly IMigrationContext _context;

    public CreateTableBuilder(IMigrationContext context, CreateTableExpression expression)
        : base(expression) =>
        _context = context;

    public ColumnDefinition CurrentColumn { get; set; } = null!;

    public ForeignKeyDefinition CurrentForeignKey { get; set; } = null!;

    /// <inheritdoc />
    public void Do() => Expression.Execute();

    /// <inheritdoc />
    public ICreateTableColumnAsTypeBuilder WithColumn(string name)
    {
        var column = new ColumnDefinition
        {
            Name = name,
            TableName = Expression.TableName,
            ModificationType = ModificationType.Create,
        };
        Expression.Columns.Add(column);
        CurrentColumn = column;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder WithDefault(SystemMethods method)
    {
        CurrentColumn.DefaultValue = method;
        return this;
    }

    public ICreateTableColumnOptionBuilder WithDefaultValue(object value)
    {
        CurrentColumn.DefaultValue = value;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder Identity()
    {
        CurrentColumn.IsIdentity = true;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder Indexed() => Indexed(null);

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder Indexed(string? indexName)
    {
        CurrentColumn.IsIndexed = true;

        var index = new CreateIndexExpression(
            _context,
            new IndexDefinition
            {
                Name = indexName,
                SchemaName = Expression.SchemaName,
                TableName = Expression.TableName,
            });

        index.Index.Columns.Add(new IndexColumnDefinition { Name = CurrentColumn.Name });

        Expression.Expressions.Add(index);

        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder PrimaryKey()
    {
        CurrentColumn.IsPrimaryKey = true;

        var expression = new CreateConstraintExpression(_context, ConstraintType.PrimaryKey)
        {
            Constraint = { TableName = CurrentColumn.TableName, Columns = new[] { CurrentColumn.Name } },
        };
        Expression.Expressions.Add(expression);

        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder PrimaryKey(string primaryKeyName)
    {
        CurrentColumn.IsPrimaryKey = true;
        CurrentColumn.PrimaryKeyName = primaryKeyName;

        var expression = new CreateConstraintExpression(_context, ConstraintType.PrimaryKey)
        {
            Constraint =
            {
                ConstraintName = primaryKeyName,
                TableName = CurrentColumn.TableName,
                Columns = new[] { CurrentColumn.Name }
            },
        };
        Expression.Expressions.Add(expression);

        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder Nullable()
    {
        CurrentColumn.IsNullable = true;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder NotNullable()
    {
        CurrentColumn.IsNullable = false;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder Unique() => Unique(null);

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder Unique(string? indexName)
    {
        CurrentColumn.IsUnique = true;

        var index = new CreateIndexExpression(
            _context,
            new IndexDefinition
            {
                Name = indexName,
                SchemaName = Expression.SchemaName,
                TableName = Expression.TableName,
                IndexType = IndexTypes.UniqueNonClustered,
            });

        index.Index.Columns.Add(new IndexColumnDefinition { Name = CurrentColumn.Name });

        Expression.Expressions.Add(index);

        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionForeignKeyCascadeBuilder ForeignKey(
        string primaryTableName,
        string primaryColumnName) => ForeignKey(null, null, primaryTableName, primaryColumnName);

    /// <inheritdoc />
    public ICreateTableColumnOptionForeignKeyCascadeBuilder ForeignKey(string foreignKeyName, string primaryTableName,
        string primaryColumnName) =>
        ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);

    /// <inheritdoc />
    public ICreateTableColumnOptionForeignKeyCascadeBuilder ForeignKey(
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
                ForeignTableSchema = Expression.SchemaName,
            });

        fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);
        fk.ForeignKey.ForeignColumns.Add(CurrentColumn.Name);

        Expression.Expressions.Add(fk);
        CurrentForeignKey = fk.ForeignKey;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionForeignKeyCascadeBuilder ForeignKey()
    {
        CurrentColumn.IsForeignKey = true;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(
        string foreignTableName,
        string foreignColumnName) => ReferencedBy(null, null, foreignTableName, foreignColumnName);

    /// <inheritdoc />
    public ICreateTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(string foreignKeyName, string foreignTableName,
        string foreignColumnName) =>
        ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);

    /// <inheritdoc />
    public ICreateTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(
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
                PrimaryTableSchema = Expression.SchemaName,
                ForeignTable = foreignTableName,
                ForeignTableSchema = foreignTableSchema,
            });

        fk.ForeignKey.PrimaryColumns.Add(CurrentColumn.Name);
        fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

        Expression.Expressions.Add(fk);
        CurrentForeignKey = fk.ForeignKey;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionForeignKeyCascadeBuilder OnDelete(Rule rule)
    {
        CurrentForeignKey.OnDelete = rule;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionForeignKeyCascadeBuilder OnUpdate(Rule rule)
    {
        CurrentForeignKey.OnUpdate = rule;
        return this;
    }

    /// <inheritdoc />
    public ICreateTableColumnOptionBuilder OnDeleteOrUpdate(Rule rule)
    {
        OnDelete(rule);
        OnUpdate(rule);
        return this;
    }

    public override ColumnDefinition GetColumnForType() => CurrentColumn;
}
