using Umbraco.Core.Persistence.Migrations.Syntax.Update.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Update
{
    public class UpdateBuilder : IUpdateBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;

        public UpdateBuilder(IMigrationContext context, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _databaseProviders = databaseProviders;
        }

        public IUpdateSetSyntax Table(string tableName)
        {
            var expression = _databaseProviders == null 
                ? new UpdateDataExpression { TableName = tableName }
                : new UpdateDataExpression(_context.CurrentDatabaseProvider, _databaseProviders) { TableName = tableName };
            _context.Expressions.Add(expression);
            return new UpdateDataBuilder(expression, _context);
        }
    }
}