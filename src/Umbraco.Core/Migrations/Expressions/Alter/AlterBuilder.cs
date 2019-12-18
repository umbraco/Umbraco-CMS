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

        public AlterBuilder(IMigrationContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IAlterTableBuilder Table(string tableName)
        {
            var expression = new AlterTableExpression(_context) { TableName = tableName };
            return new AlterTableBuilder(_context, expression);
        }
    }
}
