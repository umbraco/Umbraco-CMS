using NPoco;
using Umbraco.Core.Persistence.Migrations.Syntax.Update.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Update
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

        public IUpdateSetSyntax Table(string tableName)
        {
            var expression = new UpdateDataExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            _context.Expressions.Add(expression);
            return new UpdateDataBuilder(expression, _context);
        }
    }
}