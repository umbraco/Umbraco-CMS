using NPoco;
using Umbraco.Core.Persistence.Migrations.Syntax.Insert.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert
{
    public class InsertBuilder : IInsertBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public InsertBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public IInsertDataSyntax IntoTable(string tableName)
        {
            var expression = new InsertDataExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            _context.Expressions.Add(expression);
            return new InsertDataBuilder(expression);
        }
    }
}