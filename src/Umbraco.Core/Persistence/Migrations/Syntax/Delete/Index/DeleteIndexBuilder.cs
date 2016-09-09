using System;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Index
{
    public class DeleteIndexBuilder : ExpressionBuilderBase<DeleteIndexExpression>, IDeleteIndexForTableSyntax, IDeleteIndexOnColumnSyntax
    {
        public DeleteIndexBuilder(DeleteIndexExpression expression) : base(expression)
        {
        }

        public IndexColumnDefinition CurrentColumn { get; set; }

        public IDeleteIndexOnColumnSyntax OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }

        [Obsolete("I don't think this would ever be used when dropping an index, see DeleteIndexExpression.ToString")]
        public void OnColumn(string columnName)
        {
            var column = new IndexColumnDefinition { Name = columnName };
            Expression.Index.Columns.Add(column);
        }

        [Obsolete("I don't think this would ever be used when dropping an index, see DeleteIndexExpression.ToString")]
        public void OnColumns(params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                Expression.Index.Columns.Add(new IndexColumnDefinition { Name = columnName });
            }
        }
    }
}