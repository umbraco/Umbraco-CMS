using Umbraco.Core.Migrations.Expressions.Common;
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
            var defaultConstraint = _context.SqlContext.SqlSyntax.GetDefaultConstraint(_context.Database, Expression.TableName, columnName);
            Expression.ConstraintName = defaultConstraint ?? string.Empty;
            Expression.ColumnName = columnName;
            return new ExecutableBuilder(Expression);
        }
    }
}
