using System;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Index
{
    /// <summary>
    /// Implements <see cref="IDeleteIndexForTableBuilder"/>, <see cref="IDeleteIndexOnColumnBuilder"/>.
    /// </summary>
    public class DeleteIndexBuilder : ExpressionBuilderBase<DeleteIndexExpression>, IDeleteIndexForTableBuilder, IDeleteIndexOnColumnBuilder
    {
        public DeleteIndexBuilder(DeleteIndexExpression expression)
            : base(expression)
        { }

        public IndexColumnDefinition CurrentColumn { get; set; }

        public IDeleteIndexOnColumnBuilder OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public void OnColumn(string columnName)
        {
            var column = new IndexColumnDefinition { Name = columnName };
            Expression.Index.Columns.Add(column);
            Expression.Execute();
        }

        /// <inheritdoc />
        public void OnColumns(params string[] columnNames)
        {
            foreach (string columnName in columnNames)
                Expression.Index.Columns.Add(new IndexColumnDefinition { Name = columnName });
            Expression.Execute();
        }
    }
}
