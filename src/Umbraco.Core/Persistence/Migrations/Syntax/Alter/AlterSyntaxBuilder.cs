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
            var expression = new AlterTableExpression { TableName = tableName };
            //_context.Expressions.Add(expression);
            return new AlterTableSyntaxBuilder(expression, _context);
        }

        public IAlterColumnSyntax Column(string columnName)
        {
            var expression = new AlterColumnExpression { Column = { Name = columnName } };
            //_context.Expressions.Add(expression);
            return new AlterColumnSyntaxBuilder(expression, _context);
        }
    }
}