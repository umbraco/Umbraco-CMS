using NPoco;
using Umbraco.Core.Migrations.Expressions.Alter.Expressions;
using Umbraco.Core.Migrations.Expressions.Alter.Table;

namespace Umbraco.Core.Migrations.Expressions.Alter
{
    /// <summary>
    /// Implements <see cref="IAlterBuilder"/>.
    /// </summary>
    public class AlterBuilder : IAlterBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public AlterBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        /// <inheritdoc />
        public IAlterTableBuilder Table(string tableName)
        {
            var expression = new AlterTableExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            return new AlterTableBuilder(_context, _supportedDatabaseTypes, expression);
        }
    }
}
