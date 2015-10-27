using Umbraco.Core.Persistence.Migrations.Syntax.Insert.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert
{
    public class InsertBuilder : IInsertBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public InsertBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }

        public IInsertDataSyntax IntoTable(string tableName)
        {
            var expression = new InsertDataExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { TableName = tableName };
            _context.Expressions.Add(expression);
            return new InsertDataBuilder(expression);
        }
    }
}