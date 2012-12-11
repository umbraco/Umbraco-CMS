using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename
{
    public class RenameBuilder : IRenameBuilder
    {
        private readonly IMigrationContext _context;

        public RenameBuilder(IMigrationContext context)
        {
            _context = context;
        }

        public IRenameTableSyntax Table(string oldName)
        {
            var expression = new RenameTableExpression { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameTableBuilder(expression);
        }

        public IRenameColumnTableSyntax Column(string oldName)
        {
            var expression = new RenameColumnExpression { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameColumnBuilder(expression);
        }
    }
}