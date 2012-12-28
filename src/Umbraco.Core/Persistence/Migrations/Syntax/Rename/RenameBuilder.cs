using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename
{
    public class RenameBuilder : IRenameBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;

        public RenameBuilder(IMigrationContext context, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _databaseProviders = databaseProviders;
        }

        public IRenameTableSyntax Table(string oldName)
        {
            var expression = _databaseProviders == null
                                 ? new RenameTableExpression {OldName = oldName}
                                 : new RenameTableExpression(_context.CurrentDatabaseProvider, _databaseProviders) { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameTableBuilder(expression);
        }

        public IRenameColumnTableSyntax Column(string oldName)
        {
            var expression = _databaseProviders == null
                                 ? new RenameColumnExpression {OldName = oldName}
                                 : new RenameColumnExpression(_context.CurrentDatabaseProvider, _databaseProviders) { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameColumnBuilder(expression);
        }
    }
}