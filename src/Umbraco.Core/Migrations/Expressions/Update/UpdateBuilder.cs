using NPoco;
using Umbraco.Core.Migrations.Expressions.Update.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Update
{
    public class UpdateBuilder : IUpdateBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public UpdateBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        /// <inheritdoc />
        public IUpdateTableBuilder Table(string tableName)
        {
            var expression = new UpdateDataExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            return new UpdateDataBuilder(expression);
        }
    }
}
