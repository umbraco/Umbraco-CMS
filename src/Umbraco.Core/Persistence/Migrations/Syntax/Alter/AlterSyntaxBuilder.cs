using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter
{
    public class AlterSyntaxBuilder : IAlterSyntaxBuilder
    {
        private readonly IMigrationContext _context;

        public AlterSyntaxBuilder(IMigrationContext context)
        {
            _context = context;
        }

        public IAlterTableSyntax Table(string tableName)
        {
            var expression = new AlterTableExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider) { TableName = tableName };
            //_context.Expressions.Add(expression);
            return new AlterTableBuilder(expression, _context);
        }

        public IAlterColumnSyntax Column(string columnName)
        {
            var expression = new AlterColumnExpression(_context.SqlSyntax, _context.CurrentDatabaseProvider) { Column = { Name = columnName } };
            //_context.Expressions.Add(expression);
            return new AlterColumnBuilder(expression, _context);
        }
    }
}