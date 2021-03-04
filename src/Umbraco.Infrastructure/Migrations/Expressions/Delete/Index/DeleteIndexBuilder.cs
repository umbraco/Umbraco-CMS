﻿using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Index
{
    public class DeleteIndexBuilder : ExpressionBuilderBase<DeleteIndexExpression>,
        IDeleteIndexForTableBuilder, IExecutableBuilder
    {
        public DeleteIndexBuilder(DeleteIndexExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public void Do() => Expression.Execute();

        public IExecutableBuilder OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }
    }
}
