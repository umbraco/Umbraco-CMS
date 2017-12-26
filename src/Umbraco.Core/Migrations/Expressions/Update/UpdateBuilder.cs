using NPoco;
using Umbraco.Core.Migrations.Expressions.Update.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Update
{
    public class UpdateBuilder : IUpdateBuilder
    {
        private readonly IMigrationContext _context;

        public UpdateBuilder(IMigrationContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IUpdateTableBuilder Table(string tableName)
        {
            var expression = new UpdateDataExpression(_context) { TableName = tableName };
            return new UpdateDataBuilder(expression);
        }
    }
}
