using System.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;

/// <summary>
/// Provides a fluent builder for defining and configuring a new column as part of a database migration.
/// </summary>
public class CreateColumnBuilder : ExpressionBuilderBase<CreateColumnExpression, ICreateColumnOptionBuilder>,
    ICreateColumnOnTableBuilder,
    ICreateColumnTypeBuilder,
    ICreateColumnOptionForeignKeyCascadeBuilder
{
    private readonly IMigrationContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateColumnBuilder"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="expression">The create column expression.</param>
    public CreateColumnBuilder(IMigrationContext context, CreateColumnExpression expression)
        : base(expression) =>
        _context = context;

    /// <summary>
    /// Gets or sets the current foreign key definition associated with the column.
    /// </summary>
    public ForeignKeyDefinition? CurrentForeignKey { get; set; }

    /// <summary>
    /// Specifies the table name for the column being created.
    /// </summary>
    /// <param name="name">The name of the table.</param>
    /// <returns>An <see cref="ICreateColumnTypeBuilder"/> to continue building the column.</returns>
    public ICreateColumnTypeBuilder OnTable(string name)
    {
        Expression.TableName = name;
        return this;
    }

    /// <summary>
    /// Executes the associated create column expression for the migration.
    /// </summary>
    public void Do() => Expression.Execute();

    /// <summary>
    /// Sets the default value of the column to the specified system method.
    /// </summary>
    /// <param name="method">The system method to use as the default value.</param>
    /// <returns>An object to continue building column options.</returns>
    public ICreateColumnOptionBuilder WithDefault(SystemMethods method)
    {
        Expression.Column.DefaultValue = method;
        return this;
    }

    /// <summary>
    /// Sets the default value for the column.
    /// </summary>
    /// <param name="value">The default value to set for the column.</param>
    /// <returns>An <see cref="ICreateColumnOptionBuilder"/> to continue building the column options.</returns>
    public ICreateColumnOptionBuilder WithDefaultValue(object value)
    {
        Expression.Column.DefaultValue = value;
        return this;
    }

    /// <summary>
    /// Configures the column as an identity column, meaning its value will be automatically generated and incremented by the database.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column.ICreateColumnOptionBuilder" /> that can be used to further configure the column.</returns>
    public ICreateColumnOptionBuilder Identity() => Indexed(null);

    /// <summary>Marks the column as indexed.</summary>
    /// <returns>An <see cref="ICreateColumnOptionBuilder"/> to allow further configuration of the column.</returns>
    public ICreateColumnOptionBuilder Indexed() => Indexed(null);

    /// <summary>
    /// Specifies that the column should be indexed with an optional index name.
    /// </summary>
    /// <param name="indexName">The name of the index to create. If null, a default name will be used.</param>
    /// <returns>An <see cref="ICreateColumnOptionBuilder"/> to continue building the column options.</returns>
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

    /// <summary>
    /// Marks the column as a primary key.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column.ICreateColumnOptionBuilder" /> to allow further configuration of the column.</returns>
    public ICreateColumnOptionBuilder PrimaryKey()
    {
        Expression.Column.IsPrimaryKey = true;
        return this;
    }

    /// <summary>
    /// Marks the column as a primary key with the specified constraint name.
    /// </summary>
    /// <param name="primaryKeyName">The name of the primary key constraint.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column.ICreateColumnOptionBuilder" /> that can be used for further column configuration.</returns>
    public ICreateColumnOptionBuilder PrimaryKey(string primaryKeyName)
    {
        Expression.Column.IsPrimaryKey = true;
        Expression.Column.PrimaryKeyName = primaryKeyName;
        return this;
    }

    /// <summary>
    /// Marks the column as nullable.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column.ICreateColumnOptionBuilder" /> to continue building the column options.</returns>
    public ICreateColumnOptionBuilder Nullable()
    {
        Expression.Column.IsNullable = true;
        return this;
    }

    /// <summary>
    /// Marks the column as not nullable.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column.ICreateColumnOptionBuilder" /> to continue building the column.</returns>
    public ICreateColumnOptionBuilder NotNullable()
    {
        Expression.Column.IsNullable = false;
        return this;
    }

    /// <summary>
    /// Specifies that the column should have a unique constraint, ensuring all values in the column are distinct.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column.ICreateColumnOptionBuilder" /> for further column option configuration.</returns>
    public ICreateColumnOptionBuilder Unique() => Unique(null);

    /// <summary>
    /// Specifies that the column should have a unique index.
    /// </summary>
    /// <param name="indexName">The name of the unique index. If null, a default name will be used.</param>
    /// <returns>An <see cref="ICreateColumnOptionBuilder"/> to continue building the column options.</returns>
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

    /// <summary>
    /// Defines a foreign key constraint on the column referencing the specified primary table and column.
    /// </summary>
    /// <param name="primaryTableName">The name of the primary table that the foreign key references.</param>
    /// <param name="primaryColumnName">The name of the primary column in the referenced table.</param>
    /// <returns>A builder to specify foreign key cascade options.</returns>
    public ICreateColumnOptionForeignKeyCascadeBuilder ForeignKey(string primaryTableName, string primaryColumnName) =>
        ForeignKey(null, null, primaryTableName, primaryColumnName);

    /// <summary>
    /// Defines a foreign key constraint on the column, using the specified constraint name, and referencing the given primary table and column.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint.</param>
    /// <param name="primaryTableName">The name of the primary table referenced by the foreign key.</param>
    /// <param name="primaryColumnName">The name of the primary column in the referenced table.</param>
    /// <returns>An <see cref="ICreateColumnOptionForeignKeyCascadeBuilder"/> to specify cascade options for the foreign key.</returns>
    public ICreateColumnOptionForeignKeyCascadeBuilder ForeignKey(
        string foreignKeyName,
        string primaryTableName,
        string primaryColumnName) =>
        ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);

    /// <summary>
    /// Defines a foreign key constraint for the column being created.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint. Can be null.</param>
    /// <param name="primaryTableSchema">The schema of the primary table. Can be null.</param>
    /// <param name="primaryTableName">The name of the primary table.</param>
    /// <param name="primaryColumnName">The name of the primary column in the primary table.</param>
    /// <returns>A builder to specify cascade options for the foreign key.</returns>
    public ICreateColumnOptionForeignKeyCascadeBuilder ForeignKey(
        string? foreignKeyName,
        string? primaryTableSchema,
        string primaryTableName,
        string primaryColumnName)
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

    /// <summary>
    /// Specifies a foreign key constraint for the column referencing the given primary table and column.
    /// </summary>
    /// <returns>An object for configuring cascade options on the foreign key constraint.</returns>
    public ICreateColumnOptionForeignKeyCascadeBuilder ForeignKey()
    {
        Expression.Column.IsForeignKey = true;
        return this;
    }

    /// <summary>
    /// Specifies the foreign key relationship by referencing a foreign table and column.
    /// </summary>
    /// <param name="foreignTableName">The name of the foreign table to reference.</param>
    /// <param name="foreignColumnName">The name of the foreign column to reference.</param>
    /// <returns>An <see cref="ICreateColumnOptionForeignKeyCascadeBuilder"/> to allow further configuration of the foreign key cascade options.</returns>
    public ICreateColumnOptionForeignKeyCascadeBuilder
        ReferencedBy(string foreignTableName, string foreignColumnName) =>
        ReferencedBy(null, null, foreignTableName, foreignColumnName);

    /// <summary>
    /// Specifies the foreign key relationship by referencing a foreign table and column with a specific foreign key name.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint.</param>
    /// <param name="foreignTableName">The name of the foreign table to reference.</param>
    /// <param name="foreignColumnName">The name of the foreign column to reference.</param>
    /// <returns>An <see cref="ICreateColumnOptionForeignKeyCascadeBuilder"/> to allow further configuration of the foreign key cascade options.</returns>
    public ICreateColumnOptionForeignKeyCascadeBuilder ReferencedBy(
        string foreignKeyName,
        string foreignTableName,
        string foreignColumnName) =>
        ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);

    /// <summary>
    /// Defines a foreign key constraint referencing the specified table and column.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint. Can be null.</param>
    /// <param name="foreignTableSchema">The schema of the foreign table. Can be null.</param>
    /// <param name="foreignTableName">The name of the foreign table.</param>
    /// <param name="foreignColumnName">The name of the foreign column.</param>
    /// <returns>An <see cref="ICreateColumnOptionForeignKeyCascadeBuilder"/> to specify cascade options.</returns>
    public ICreateColumnOptionForeignKeyCascadeBuilder ReferencedBy(
        string? foreignKeyName,
        string? foreignTableSchema,
        string foreignTableName,
        string foreignColumnName)
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

    /// <summary>
    /// Specifies the action to take when a referenced row in the foreign key is deleted.
    /// </summary>
    /// <param name="rule">The <see cref="Rule"/> that determines the delete behavior for the foreign key constraint.</param>
    /// <returns>An <see cref="ICreateColumnOptionForeignKeyCascadeBuilder"/> to continue configuring the column options.</returns>
    public ICreateColumnOptionForeignKeyCascadeBuilder OnDelete(Rule rule)
    {
        if (CurrentForeignKey is not null)
        {
            CurrentForeignKey.OnDelete = rule;
        }

        return this;
    }

    /// <summary>
    /// Sets the ON UPDATE rule for the foreign key constraint.
    /// </summary>
    /// <param name="rule">The rule to apply on update.</param>
    /// <returns>The current <see cref="ICreateColumnOptionForeignKeyCascadeBuilder"/> instance for chaining.</returns>
    public ICreateColumnOptionForeignKeyCascadeBuilder OnUpdate(Rule rule)
    {
        if (CurrentForeignKey is not null)
        {
            CurrentForeignKey.OnUpdate = rule;
        }

        return this;
    }

    /// <summary>
    /// Sets the specified referential action rule to apply to both delete and update operations for the foreign key constraint on this column.
    /// </summary>
    /// <param name="rule">The <see cref="Rule"/> to apply for both delete and update actions (e.g., Cascade, SetNull).</param>
    /// <returns>An <see cref="ICreateColumnOptionBuilder"/> that can be used to continue configuring the column options.</returns>
    public ICreateColumnOptionBuilder OnDeleteOrUpdate(Rule rule)
    {
        OnDelete(rule);
        OnUpdate(rule);
        return this;
    }

    /// <summary>
    /// Returns the column definition associated with the current column expression.
    /// </summary>
    /// <returns>The <see cref="ColumnDefinition"/> for the current column.</returns>
    public override ColumnDefinition GetColumnForType() => Expression.Column;
}
