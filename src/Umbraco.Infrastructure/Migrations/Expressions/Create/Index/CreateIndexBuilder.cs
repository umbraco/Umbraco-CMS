using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;

public class CreateIndexBuilder : ExpressionBuilderBase<CreateIndexExpression>,
    ICreateIndexForTableBuilder,
    ICreateIndexOnColumnBuilder,
    ICreateIndexColumnOptionsBuilder,
    ICreateIndexOptionsBuilder
{
    public CreateIndexBuilder(CreateIndexExpression expression)
        : base(expression)
    {
    }

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
