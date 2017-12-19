using NPoco;
using Umbraco.Core.Migrations.Expressions.Insert.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Insert
{
    /// <summary>
    /// Implements <see cref="IInsertBuilder"/>.
    /// </summary>
    public class InsertBuilder : IInsertBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public InsertBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        /// <inheritdoc />
        public IInsertIntoBuilder IntoTable(string tableName)
        {
            var expression = new InsertDataExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            return new InsertIntoBuilder(expression);
        }
    }
}
