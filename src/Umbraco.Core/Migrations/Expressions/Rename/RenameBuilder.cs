using NPoco;
using Umbraco.Core.Migrations.Expressions.Rename.Column;
using Umbraco.Core.Migrations.Expressions.Rename.Expressions;
using Umbraco.Core.Migrations.Expressions.Rename.Table;

namespace Umbraco.Core.Migrations.Expressions.Rename
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

        /// <inheritdoc />
        public IRenameTableBuilder Table(string oldName)
        {
            var expression = new RenameTableExpression(_context, _supportedDatabaseTypes) { OldName = oldName };
            return new RenameTableBuilder(expression);
        }

        /// <inheritdoc />
        public IRenameColumnBuilder Column(string oldName)
        {
            var expression = new RenameColumnExpression(_context, _supportedDatabaseTypes) { OldName = oldName };
            return new RenameColumnBuilder(expression);
        }
    }
}
