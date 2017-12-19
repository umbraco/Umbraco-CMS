using Umbraco.Core.Migrations.Expressions.Common.Expressions;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Create.Index
{
    public class CreateIndexBuilder : ExpressionBuilderBase<CreateIndexExpression>,
                                      ICreateIndexForTableBuilder,
                                      ICreateIndexOnColumnBuilder,
                                      ICreateIndexColumnOptionsBuilder,
                                      ICreateIndexOptionsBuilder
    {
        public CreateIndexBuilder(CreateIndexExpression expression) : base(expression)
        {
        }

        public IndexColumnDefinition CurrentColumn { get; set; }

        public ICreateIndexOnColumnBuilder OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }

        public ICreateIndexColumnOptionsBuilder OnColumn(string columnName)
        {
            CurrentColumn = new IndexColumnDefinition { Name = columnName };
            Expression.Index.Columns.Add(CurrentColumn);
            return this;
        }

        public ICreateIndexOptionsBuilder WithOptions()
        {
            return this;
        }

        public ICreateIndexOnColumnBuilder Ascending()
        {
            CurrentColumn.Direction = Direction.Ascending;
            return this;
        }

        public ICreateIndexOnColumnBuilder Descending()
        {
            CurrentColumn.Direction = Direction.Descending;
            return this;
        }

        ICreateIndexOnColumnBuilder ICreateIndexColumnOptionsBuilder.Unique()
        {
            Expression.Index.IsUnique = true;
            //if it is Unique then it must be unique nonclustered and set the other flags
            Expression.Index.IndexType = IndexTypes.UniqueNonClustered;
            Expression.Index.IsClustered = false;
            return this;
        }

        public ICreateIndexOnColumnBuilder NonClustered()
        {
            Expression.Index.IndexType = IndexTypes.NonClustered;
            Expression.Index.IsClustered = false;
            Expression.Index.IndexType = IndexTypes.NonClustered;
            Expression.Index.IsUnique = false;
            return this;
        }

        public ICreateIndexOnColumnBuilder Clustered()
        {
            Expression.Index.IndexType = IndexTypes.Clustered;
            Expression.Index.IsClustered = true;
            //if it is clustered then we have to change the index type set the other flags
            Expression.Index.IndexType = IndexTypes.Clustered;
            Expression.Index.IsClustered = true;
            Expression.Index.IsUnique = false;
            return this;
        }

        ICreateIndexOnColumnBuilder ICreateIndexOptionsBuilder.Unique()
        {
            Expression.Index.IndexType = IndexTypes.UniqueNonClustered;
            Expression.Index.IsUnique = true;
            return this;
        }
    }
}
