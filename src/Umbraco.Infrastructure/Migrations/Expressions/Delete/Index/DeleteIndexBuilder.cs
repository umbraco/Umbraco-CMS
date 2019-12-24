using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Index
{
    public class DeleteIndexBuilder : ExpressionBuilderBase<DeleteIndexExpression>,
        IDeleteIndexForTableBuilder, IDeleteIndexOnColumnBuilder
    {
        public DeleteIndexBuilder(DeleteIndexExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public void Do() => Expression.Execute();

        public IndexColumnDefinition CurrentColumn { get; set; }

        public IDeleteIndexOnColumnBuilder OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public IExecutableBuilder OnColumn(string columnName)
        {
            var column = new IndexColumnDefinition { Name = columnName };
            Expression.Index.Columns.Add(column);
            return new ExecutableBuilder(Expression);
        }

        /// <inheritdoc />
        public IExecutableBuilder OnColumns(params string[] columnNames)
        {
            foreach (string columnName in columnNames)
                Expression.Index.Columns.Add(new IndexColumnDefinition { Name = columnName });
            return new ExecutableBuilder(Expression);
        }
    }
}
