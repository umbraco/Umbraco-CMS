using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public class CheckConstraintBuilder : ExpressionBuilderBase<CheckConstraintExpression>, ICheckConstraintOptionSyntax
    {
        private IMigrationContext _context;
        private DatabaseProviders[] _databaseProviders;
        private ISqlSyntaxProvider _sqlSyntax;

        public CheckConstraintBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckConstraintExpression expression) : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public ICheckColumnConstraintOptionSyntax OnColumn(string columnName)
        {
            var expression = new CheckConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax)
            {
                ColumnName = columnName,
                ConstraintName = Expression.ConstraintName
            };

            return new CheckColumnConstraintBuilder(_context, _databaseProviders, _sqlSyntax, expression);
        }

        public ICheckTableConstraintOptionSyntax OnTable(string tableName)
        {
            var expression = new CheckConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax)
            {
                ConstraintName = Expression.ConstraintName,
                TableName = tableName
            };

            return new CheckTableConstraintBuilder(_context, _databaseProviders, _sqlSyntax, expression);
        }
    }
}
