using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Index
{
    public class CreateIndexBuilder : ExpressionBuilderBase<CreateIndexExpression>,
                                      ICreateIndexForTableSyntax,
                                      ICreateIndexOnColumnSyntax,
                                      ICreateIndexColumnOptionsSyntax,
                                      ICreateIndexOptionsSyntax
    {
        public CreateIndexBuilder(CreateIndexExpression expression) : base(expression)
        {
        }

        public IndexColumnDefinition CurrentColumn { get; set; }

        public ICreateIndexOnColumnSyntax OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }

        public ICreateIndexColumnOptionsSyntax OnColumn(string columnName)
        {
            CurrentColumn = new IndexColumnDefinition { Name = columnName };
            Expression.Index.Columns.Add(CurrentColumn);
            return this;
        }

        public ICreateIndexOptionsSyntax WithOptions()
        {
            return this;
        }

        public ICreateIndexOnColumnSyntax Ascending()
        {
            CurrentColumn.Direction = Direction.Ascending;
            return this;
        }

        public ICreateIndexOnColumnSyntax Descending()
        {
            CurrentColumn.Direction = Direction.Descending;
            return this;
        }

        ICreateIndexOnColumnSyntax ICreateIndexColumnOptionsSyntax.Unique()
        {
            Expression.Index.IsUnique = true;
            //if it is Unique then it must be unique nonclustered and set the other flags
            Expression.Index.IndexType = IndexTypes.UniqueNonClustered;
            Expression.Index.IsClustered = false;
            return this;
        }

        public ICreateIndexOnColumnSyntax NonClustered()
        {
            Expression.Index.IndexType = IndexTypes.NonClustered;
            Expression.Index.IsClustered = false;
            Expression.Index.IndexType = IndexTypes.NonClustered;
            Expression.Index.IsUnique = false;
            return this;
        }

        public ICreateIndexOnColumnSyntax Clustered()
        {
            Expression.Index.IndexType = IndexTypes.Clustered;
            Expression.Index.IsClustered = true;
            //if it is clustered then we have to change the index type set the other flags
            Expression.Index.IndexType = IndexTypes.Clustered;
            Expression.Index.IsClustered = true;
            Expression.Index.IsUnique = false;
            return this;
        }

        ICreateIndexOnColumnSyntax ICreateIndexOptionsSyntax.Unique()
        {
            Expression.Index.IndexType = IndexTypes.UniqueNonClustered;
            Expression.Index.IsUnique = true;
            return this;
        }
    }
}