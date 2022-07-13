using System.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;

public class CreateColumnBuilder : ExpressionBuilderBase<CreateColumnExpression, ICreateColumnOptionBuilder>,
    ICreateColumnOnTableBuilder,
    ICreateColumnTypeBuilder,
    ICreateColumnOptionForeignKeyCascadeBuilder
{
    private readonly IMigrationContext _context;

    public CreateColumnBuilder(IMigrationContext context, CreateColumnExpression expression)
        : base(expression) =>
        _context = context;

    public ForeignKeyDefinition? CurrentForeignKey { get; set; }

    public ICreateColumnTypeBuilder OnTable(string name)
    {
        Expression.TableName = name;
        return this;
    }

    public void Do() => Expression.Execute();

    public ICreateColumnOptionBuilder WithDefault(SystemMethods method)
    {
        Expression.Column.DefaultValue = method;
        return this;
    }

    public ICreateColumnOptionBuilder WithDefaultValue(object value)
    {
        Expression.Column.DefaultValue = value;
        return this;
    }

    public ICreateColumnOptionBuilder Identity() => Indexed(null);

    public ICreateColumnOptionBuilder Indexed() => Indexed(null);

    public ICreateColumnOptionBuilder Indexed(string? indexName)
    {
        Expression.Column.IsIndexed = true;

        var index = new CreateIndexExpression(
            _context,
            new IndexDefinition { Name = indexName, TableName = Expression.TableName });

        index.Index.Columns.Add(new IndexColumnDefinition { Name = Expression.Column.Name });

        Expression.Expressions.Add(index);

        return this;
    }

    public ICreateColumnOptionBuilder PrimaryKey()
    {
        Expression.Column.IsPrimaryKey = true;
        return this;
    }

    public ICreateColumnOptionBuilder PrimaryKey(string primaryKeyName)
    {
        Expression.Column.IsPrimaryKey = true;
        Expression.Column.PrimaryKeyName = primaryKeyName;
        return this;
    }

    public ICreateColumnOptionBuilder Nullable()
    {
        Expression.Column.IsNullable = true;
        return this;
    }

    public ICreateColumnOptionBuilder NotNullable()
    {
        Expression.Column.IsNullable = false;
        return this;
    }

    public ICreateColumnOptionBuilder Unique() => Unique(null);

    public ICreateColumnOptionBuilder Unique(string? indexName)
    {
        Expression.Column.IsUnique = true;

        var index = new CreateIndexExpression(
            _context,
            new IndexDefinition
            {
                Name = indexName,
                TableName = Expression.TableName,
                IndexType = IndexTypes.UniqueNonClustered,
            });

        index.Index.Columns.Add(new IndexColumnDefinition { Name = Expression.Column.Name });

        Expression.Expressions.Add(index);

        return this;
    }

    public ICreateColumnOptionForeignKeyCascadeBuilder ForeignKey(string primaryTableName, string primaryColumnName) =>
        ForeignKey(null, null, primaryTableName, primaryColumnName);

    public ICreateColumnOptionForeignKeyCascadeBuilder ForeignKey(string foreignKeyName, string primaryTableName,
        string primaryColumnName) =>
        ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);

    public ICreateColumnOptionForeignKeyCascadeBuilder ForeignKey(string? foreignKeyName, string? primaryTableSchema,
        string primaryTableName, string primaryColumnName)
    {
        Expression.Column.IsForeignKey = true;

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
        fk.ForeignKey.ForeignColumns.Add(Expression.Column.Name);

        Expression.Expressions.Add(fk);
        CurrentForeignKey = fk.ForeignKey;
        return this;
    }

    public ICreateColumnOptionForeignKeyCascadeBuilder ForeignKey()
    {
        Expression.Column.IsForeignKey = true;
        return this;
    }

    public ICreateColumnOptionForeignKeyCascadeBuilder
        ReferencedBy(string foreignTableName, string foreignColumnName) =>
        ReferencedBy(null, null, foreignTableName, foreignColumnName);

    public ICreateColumnOptionForeignKeyCascadeBuilder ReferencedBy(string foreignKeyName, string foreignTableName,
        string foreignColumnName) =>
        ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);

    public ICreateColumnOptionForeignKeyCascadeBuilder ReferencedBy(string? foreignKeyName, string? foreignTableSchema,
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

        fk.ForeignKey.PrimaryColumns.Add(Expression.Column.Name);
        fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

        Expression.Expressions.Add(fk);
        CurrentForeignKey = fk.ForeignKey;
        return this;
    }

    public ICreateColumnOptionForeignKeyCascadeBuilder OnDelete(Rule rule)
    {
        if (CurrentForeignKey is not null)
        {
            CurrentForeignKey.OnDelete = rule;
        }

        return this;
    }

    public ICreateColumnOptionForeignKeyCascadeBuilder OnUpdate(Rule rule)
    {
        if (CurrentForeignKey is not null)
        {
            CurrentForeignKey.OnUpdate = rule;
        }

        return this;
    }

    public ICreateColumnOptionBuilder OnDeleteOrUpdate(Rule rule)
    {
        OnDelete(rule);
        OnUpdate(rule);
        return this;
    }

    public override ColumnDefinition GetColumnForType() => Expression.Column;
}
