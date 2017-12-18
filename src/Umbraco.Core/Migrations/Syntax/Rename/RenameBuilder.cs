using NPoco;
using Umbraco.Core.Migrations.Syntax.Rename.Column;
using Umbraco.Core.Migrations.Syntax.Rename.Expressions;
using Umbraco.Core.Migrations.Syntax.Rename.Table;

namespace Umbraco.Core.Migrations.Syntax.Rename
{
    public class RenameBuilder : IRenameBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public RenameBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public IRenameTableSyntax Table(string oldName)
        {
            var expression = new RenameTableExpression(_context, _supportedDatabaseTypes) { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameTableBuilder(expression);
        }

        public IRenameColumnTableSyntax Column(string oldName)
        {
            var expression = new RenameColumnExpression(_context, _supportedDatabaseTypes) { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameColumnBuilder(expression);
        }
    }
}
