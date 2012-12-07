using Umbraco.Core.Persistence.Migrations.Syntax.Update.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Update
{
    public class UpdateBuilder : IUpdateBuilder
    {
        private readonly IMigrationContext _context;

        public UpdateBuilder(IMigrationContext context)
        {
            _context = context;
        }

        public IUpdateSetSyntax Table(string tableName)
        {
            var expression = new UpdateDataExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new UpdateDataBuilder(expression, _context);
        }
    }
}