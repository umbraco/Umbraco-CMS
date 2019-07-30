﻿using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Delete.DefaultConstraint
{
    /// <summary>
    /// Implements <see cref="IDeleteDefaultConstraintOnTableBuilder"/>, <see cref="IDeleteDefaultConstraintOnColumnBuilder"/>.
    /// </summary>
    public class DeleteDefaultConstraintBuilder : ExpressionBuilderBase<DeleteDefaultConstraintExpression>,
        IDeleteDefaultConstraintOnTableBuilder,
        IDeleteDefaultConstraintOnColumnBuilder
    {
        private readonly IMigrationContext _context;

        public DeleteDefaultConstraintBuilder(IMigrationContext context, DeleteDefaultConstraintExpression expression)
            : base(expression)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IDeleteDefaultConstraintOnColumnBuilder OnTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public IExecutableBuilder OnColumn(string columnName)
        {
            Expression.ColumnName = columnName;
            Expression.HasDefaultConstraint = _context.SqlContext.SqlSyntax.TryGetDefaultConstraint(_context.Database, Expression.TableName, columnName, out var constraintName);
            Expression.ConstraintName = constraintName ?? string.Empty;

            return new ExecutableBuilder(Expression);
        }
    }
}
