using Umbraco.Core.Persistence.Migrations.Syntax.Check.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Table
{
    public class CheckTableBuilder : ExpressionBuilderBase<CheckTableExpression>, ICheckTableSyntax
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public CheckTableBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, CheckTableExpression expression)
            : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
            _sqlSyntax = sqlSyntax;
        }

        public bool Exists()
        {
            return _sqlSyntax.DoesTableExist(_context.Database, Expression.TableName);
        }

        public ICheckColumnOnTableSyntax WithColumn(string columnName)
        {
            var expression = new CheckColumnExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax)
            {
                ColumnName = columnName,
                TableName = Expression.TableName
            };

            return new CheckColumnBuilder(_context, _sqlSyntax, expression);
        }

        public ICheckOptionSyntax WithColumns(string[] columnNames)
        {
            var expression = new CheckColumnsExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax)
            {
                ColumnNames = columnNames,
                TableName = Expression.TableName
            };

            return new CheckColumnsBuilder(_context, _sqlSyntax, expression);
        }
    }
}
