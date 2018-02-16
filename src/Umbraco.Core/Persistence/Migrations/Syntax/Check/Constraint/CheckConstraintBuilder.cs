using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public class CheckConstraintBuilder : ExpressionBuilderBase<CheckConstraintExpression>, ICheckConstraintSyntax
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

        public ICheckColumnsConstraintOptionSyntax OnColumns(string[] columnNames)
        {
            var expression = new CheckConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax)
            {
                ColumnNames = columnNames,
                ConstraintName = Expression.ConstraintName
            };

            return new CheckColumnsConstraintBuilder(_context, _databaseProviders, _sqlSyntax, expression);
        }

        public ICheckColumnsConstraintOptionSyntax OnColumn(string columnName)
        {
            var columnNames = new string[] { columnName };

            return OnColumns(columnNames);
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
