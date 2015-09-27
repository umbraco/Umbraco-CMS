using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter
{
    public class AlterSyntaxBuilder : IAlterSyntaxBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public AlterSyntaxBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }

        public IAlterTableSyntax Table(string tableName)
        {
            var expression = new AlterTableExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { TableName = tableName };
            //_context.Expressions.Add(expression);
            return new AlterTableBuilder(_context, _databaseProviders, expression);
        }

        public IAlterColumnSyntax Column(string columnName)
        {
            var expression = new AlterColumnExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) {Column = {Name = columnName}};
            //_context.Expressions.Add(expression);
            return new AlterColumnBuilder(_context, _databaseProviders, expression);
        }
    }
}