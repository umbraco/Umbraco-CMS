using NPoco;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter
{
    public class AlterSyntaxBuilder : IAlterSyntaxBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public AlterSyntaxBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public IAlterTableSyntax Table(string tableName)
        {
            var expression = new AlterTableExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            //_context.Expressions.Add(expression);
            return new AlterTableBuilder(_context, _supportedDatabaseTypes, expression);
        }

        public IAlterColumnSyntax Column(string columnName)
        {
            var expression = new AlterColumnExpression(_context, _supportedDatabaseTypes) {Column = {Name = columnName}};
            //_context.Expressions.Add(expression);
            return new AlterColumnBuilder(_context, _supportedDatabaseTypes, expression);
        }
    }
}