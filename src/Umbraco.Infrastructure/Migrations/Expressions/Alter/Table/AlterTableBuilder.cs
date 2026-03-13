using System.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table;

/// <summary>
/// Provides a fluent builder for constructing 'ALTER TABLE' expressions used in database migration operations.
/// </summary>
public class AlterTableBuilder : ExpressionBuilderBase<AlterTableExpression, IAlterTableColumnOptionBuilder>,
    IAlterTableColumnTypeBuilder,
    IAlterTableColumnOptionForeignKeyCascadeBuilder
{
    private readonly IMigrationContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlterTableBuilder"/> class.
    /// </summary>
    /// <param name="context">The migration context used for the operation. (<see cref="IMigrationContext"/>)</param>
    /// <param name="expression">The expression representing the table alteration. (<see cref="AlterTableExpression"/>)</param>
    public AlterTableBuilder(IMigrationContext context, AlterTableExpression expression)
        : base(expression) =>
        _context = context;

    /// <summary>
    /// Gets or sets the current column definition being altered.
    /// </summary>
    public ColumnDefinition CurrentColumn { get; set; } = null!;

    /// <summary>
    /// Gets or sets the definition of the foreign key currently being altered in the table.
    /// </summary>
    public ForeignKeyDefinition CurrentForeignKey { get; set; } = null!;

    /// <summary>
    /// Executes the alter table operation for the current migration expression.
    /// <para>
    /// Throws a <see cref="NotSupportedException"/> if the underlying database is SQLite, as direct ALTER TABLE operations are not supported on that platform.
    /// </para>
    /// </summary>
    public void Do()
    {
        if (_context.Database.DatabaseType.IsSqlite())
        {
            throw new NotSupportedException($"SQLite does not support ALTER TABLE operations. Instead you will have to:{Environment.NewLine}1. Create a temp table.{Environment.NewLine}2. Copy data from existing table into the temp table.{Environment.NewLine}3. Delete the existing table.{Environment.NewLine}4. Create a new table with the name of the table you're trying to alter, but with a new signature{Environment.NewLine}5. Copy data from the temp table into the new table.{Environment.NewLine}6. Delete the temp table.");
        }

        Expression.Execute();
    }

    /// <summary>
    /// Sets the default value of the current column to the specified <see cref="SystemMethods"/> value.
    /// </summary>
    /// <param name="method">The <see cref="SystemMethods"/> value to use as the default for the column.</param>
    /// <returns>An <see cref="IAlterTableColumnOptionBuilder"/> that can be used to further configure the column options.</returns>
    public IAlterTableColumnOptionBuilder WithDefault(SystemMethods method)
    {
        CurrentColumn.DefaultValue = method;
        return this;
    }

    /// <summary>
    /// Sets the default value for the column being altered.
    /// </summary>
    /// <param name="value">The default value to set for the column.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnOptionBuilder" /> to allow further configuration.</returns>
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

    /// <summary>
    /// Marks the current column as an identity column.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnOptionBuilder" /> to continue building the column options.</returns>
    public IAlterTableColumnOptionBuilder Identity()
    {
        CurrentColumn.IsIdentity = true;
        return this;
    }

    /// <summary>
    /// Specifies that the column should be indexed.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnOptionBuilder"/> that can be used to continue configuring the column alteration.</returns>
    public IAlterTableColumnOptionBuilder Indexed() => Indexed(null);

    /// <summary>
    /// Specifies that the column should be indexed with an optional index name.
    /// </summary>
    /// <param name="indexName">The optional name of the index.</param>
    /// <returns>An <see cref="IAlterTableColumnOptionBuilder"/> to continue building the column options.</returns>
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

    /// <summary>
    /// Sets the current column as the primary key for the table in the alter table expression.
    /// </summary>
    /// <returns>
    /// An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnOptionBuilder" /> that can be used to further configure the column.
    /// </returns>
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

    /// <summary>
    /// Sets the primary key constraint on the current column of the table being altered.
    /// </summary>
    /// <returns>An <see cref="IAlterTableColumnOptionBuilder"/> to continue building column options.</returns>
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

    /// <summary>
    /// Sets the current column to allow null values.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnOptionBuilder"/> for further column alteration options.</returns>
    public IAlterTableColumnOptionBuilder Nullable()
    {
        CurrentColumn.IsNullable = true;
        return this;
    }

    /// <summary>
    /// Specifies that the current column cannot contain null values.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnOptionBuilder" /> for further column alteration options.</returns>
    public IAlterTableColumnOptionBuilder NotNullable()
    {
        CurrentColumn.IsNullable = false;
        return this;
    }

    /// <summary>
    /// Specifies that the column should have a unique constraint applied.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnOptionBuilder" /> for further column option configuration.</returns>
    public IAlterTableColumnOptionBuilder Unique() => Unique(null);

    /// <summary>Marks the column as unique by adding a unique index.</summary>
    /// <param name="indexName">The name of the unique index. If null, a default name will be used.</param>
    /// <returns>An <see cref="IAlterTableColumnOptionBuilder"/> to continue building the column options.</returns>
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

    /// <summary>
    /// Defines a foreign key constraint referencing the specified primary table and column.
    /// </summary>
    /// <param name="primaryTableName">The name of the primary table that the foreign key references.</param>
    /// <param name="primaryColumnName">The name of the primary column in the primary table that the foreign key references.</param>
    /// <returns>An object to configure foreign key cascade options.</returns>
    public IAlterTableColumnOptionForeignKeyCascadeBuilder
        ForeignKey(string primaryTableName, string primaryColumnName) =>
        ForeignKey(null, null, primaryTableName, primaryColumnName);

    /// <summary>Defines a foreign key constraint on the table being altered.</summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint.</param>
    /// <param name="primaryTableName">The name of the primary table that the foreign key references.</param>
    /// <param name="primaryColumnName">The name of the primary column in the primary table that the foreign key references.</param>
    /// <returns>An object to specify additional foreign key options such as cascade behavior.</returns>
    public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey(
        string foreignKeyName,
        string primaryTableName,
        string primaryColumnName) =>
        ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);

    /// <summary>
    /// Adds a foreign key constraint to the altered table.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint, or <c>null</c> to use a default name.</param>
    /// <param name="primaryTableSchema">The schema of the referenced (primary) table, or <c>null</c> to use the default schema.</param>
    /// <param name="primaryTableName">The name of the referenced (primary) table.</param>
    /// <param name="primaryColumnName">The name of the referenced column in the primary table.</param>
    /// <returns>An object for configuring cascade options for the foreign key constraint.</returns>
    public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey(
        string? foreignKeyName,
        string? primaryTableSchema,
        string primaryTableName,
        string primaryColumnName)
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

    /// <summary>
    /// Creates a foreign key constraint from the current table to the specified primary table and column.
    /// </summary>
    /// <param name="primaryTableName">The name of the primary (referenced) table.</param>
    /// <param name="primaryColumnName">The name of the primary (referenced) column.</param>
    /// <returns>An object to configure cascade options for the foreign key constraint.</returns>
    public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey()
    {
        CurrentColumn.IsForeignKey = true;
        return this;
    }

    /// <summary>
    /// Defines a foreign key relationship where this table is referenced by the specified foreign table and column.
    /// </summary>
    /// <param name="foreignTableName">The name of the foreign table that references this table.</param>
    /// <param name="foreignColumnName">The name of the foreign column in the foreign table that references this table.</param>
    /// <returns>An object to specify foreign key cascade options.</returns>
    public IAlterTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(
        string foreignTableName,
        string foreignColumnName) => ReferencedBy(null, null, foreignTableName, foreignColumnName);

    public IAlterTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(
        string foreignKeyName,
        string foreignTableName,
        string foreignColumnName) =>
        ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);

    /// <summary>
    /// Defines a foreign key relationship where this table is referenced by the specified foreign table and column.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint. Can be null.</param>
    /// <param name="foreignTableSchema">The schema of the foreign table. Can be null.</param>
    /// <param name="foreignTableName">The name of the foreign table.</param>
    /// <param name="foreignColumnName">The name of the foreign column in the foreign table.</param>
    /// <returns>An object to specify foreign key cascade options.</returns>
    public IAlterTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(
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

        fk.ForeignKey.PrimaryColumns.Add(CurrentColumn.Name);
        fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

        Expression.Expressions.Add(fk);
        CurrentForeignKey = fk.ForeignKey;
        return this;
    }

    /// <summary>
    /// Adds a new column to the table.
    /// </summary>
    /// <param name="name">The name of the column to add.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnTypeBuilder" /> to further define the column.</returns>
    public IAlterTableColumnTypeBuilder AddColumn(string name)
    {
        var column = new ColumnDefinition { Name = name, ModificationType = ModificationType.Create };
        var createColumn = new CreateColumnExpression(_context) { Column = column, TableName = Expression.TableName };

        CurrentColumn = column;

        Expression.Expressions.Add(createColumn);
        return this;
    }

    /// <summary>
    /// Alters the column with the specified name.
    /// </summary>
    /// <param name="name">The name of the column to alter.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnTypeBuilder" /> to continue building the alter column expression.</returns>
    public IAlterTableColumnTypeBuilder AlterColumn(string name)
    {
        var column = new ColumnDefinition { Name = name, ModificationType = ModificationType.Alter };
        var alterColumn = new AlterColumnExpression(_context) { Column = column, TableName = Expression.TableName };

        CurrentColumn = column;

        Expression.Expressions.Add(alterColumn);
        return this;
    }

    /// <summary>
    /// Specifies the action to take when a referenced row is deleted (the ON DELETE rule) for the current foreign key constraint.
    /// </summary>
    /// <param name="rule">The <see cref="Rule"/> that determines the ON DELETE behavior (e.g., CASCADE, SET NULL, etc.).</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table.IAlterTableColumnOptionForeignKeyCascadeBuilder"/> to continue configuring the foreign key.</returns>
    public IAlterTableColumnOptionForeignKeyCascadeBuilder OnDelete(Rule rule)
    {
        CurrentForeignKey.OnDelete = rule;
        return this;
    }

    /// <summary>
    /// Sets the ON UPDATE rule for the foreign key.
    /// </summary>
    /// <param name="rule">The rule to apply on update.</param>
    /// <returns>An object to continue building the foreign key cascade options.</returns>
    public IAlterTableColumnOptionForeignKeyCascadeBuilder OnUpdate(Rule rule)
    {
        CurrentForeignKey.OnUpdate = rule;
        return this;
    }

    /// <summary>
    /// Sets the specified rule to be applied on both delete and update actions for the table.
    /// </summary>
    /// <param name="rule">The rule to apply on delete and update.</param>
    /// <returns>An <see cref="IAlterTableColumnOptionBuilder"/> to continue building the table alteration.</returns>
    public IAlterTableColumnOptionBuilder OnDeleteOrUpdate(Rule rule)
    {
        OnDelete(rule);
        OnUpdate(rule);
        return this;
    }

    /// <summary>
    /// Gets the definition of the current column being altered.
    /// </summary>
    /// <returns>The <see cref="ColumnDefinition"/> for the current column.</returns>
    public override ColumnDefinition GetColumnForType() => CurrentColumn;
}
