using Umbraco.Core.Persistence.Migrations.Syntax.Update.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Update
{
    public class UpdateBuilder : IUpdateBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public UpdateBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }

        public IUpdateSetSyntax Table(string tableName)
        {
            var expression = new UpdateDataExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { TableName = tableName };
            _context.Expressions.Add(expression);
            return new UpdateDataBuilder(expression, _context);
        }
    }
}