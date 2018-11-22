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
            Expression.Index.IndexType = IndexTypes.UniqueNonClustered;            
            return this;
        }

        public ICreateIndexOnColumnSyntax NonClustered()
        {
            Expression.Index.IndexType = IndexTypes.NonClustered;            
            return this;
        }

        public ICreateIndexOnColumnSyntax Clustered()
        {           
           Expression.Index.IndexType = IndexTypes.Clustered;           
           return this;
        }

        ICreateIndexOnColumnSyntax ICreateIndexOptionsSyntax.Unique()
        {
            Expression.Index.IndexType = IndexTypes.UniqueNonClustered;           
            return this;
        }
    }
}
