using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;

/// <summary>
/// Provides a fluent builder for defining and creating database indexes as part of migration expressions.
/// </summary>
public class CreateIndexBuilder : ExpressionBuilderBase<CreateIndexExpression>,
    ICreateIndexForTableBuilder,
    ICreateIndexOnColumnBuilder,
    ICreateIndexColumnOptionsBuilder,
    ICreateIndexOptionsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index.CreateIndexBuilder"/> class with the specified create index expression.
    /// </summary>
    /// <param name="expression">The <see cref="CreateIndexExpression"/> that defines the index to be created.</param>
    public CreateIndexBuilder(CreateIndexExpression expression)
        : base(expression)
    {
    }

    /// <summary>
    /// Gets or sets the definition of the current column being added to the index.
    /// </summary>
    public IndexColumnDefinition? CurrentColumn { get; set; }

    /// <inheritdoc />
    public ICreateIndexOnColumnBuilder Ascending()
    {
        if (CurrentColumn is not null)
        {
            CurrentColumn.Direction = Direction.Ascending;
        }

        return this;
    }

    /// <inheritdoc />
    public ICreateIndexOnColumnBuilder Descending()
    {
        if (CurrentColumn is not null)
        {
            CurrentColumn.Direction = Direction.Descending;
        }

        return this;
    }

    /// <inheritdoc />
    ICreateIndexOnColumnBuilder ICreateIndexColumnOptionsBuilder.Unique()
    {
        Expression.Index.IndexType = IndexTypes.UniqueNonClustered;
        return this;
    }

    /// <inheritdoc />
    public ICreateIndexOnColumnBuilder OnTable(string tableName)
    {
        Expression.Index.TableName = tableName;
        return this;
    }

    /// <inheritdoc />
    public void Do() => Expression.Execute();

    /// <inheritdoc />
    public ICreateIndexColumnOptionsBuilder OnColumn(string columnName)
    {
        CurrentColumn = new IndexColumnDefinition { Name = columnName };
        Expression.Index.Columns.Add(CurrentColumn);
        return this;
    }

    /// <inheritdoc />
    public ICreateIndexOptionsBuilder WithOptions() => this;

    /// <inheritdoc />
    public ICreateIndexOnColumnBuilder NonClustered()
    {
        Expression.Index.IndexType = IndexTypes.NonClustered;
        return this;
    }

    /// <inheritdoc />
    public ICreateIndexOnColumnBuilder Clustered()
    {
        Expression.Index.IndexType = IndexTypes.Clustered;
        return this;
    }

    /// <inheritdoc />
    ICreateIndexOnColumnBuilder ICreateIndexOptionsBuilder.Unique()
    {
        Expression.Index.IndexType = IndexTypes.UniqueNonClustered;
        return this;
    }
}
