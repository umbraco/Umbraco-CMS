using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Table;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename
{
    public class RenameBuilder : IRenameBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public RenameBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }

        public IRenameTableSyntax Table(string oldName)
        {
            var expression = new RenameTableExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameTableBuilder(expression);
        }

        public IRenameColumnTableSyntax Column(string oldName)
        {
            var expression = new RenameColumnExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameColumnBuilder(expression);
        }
    }
}