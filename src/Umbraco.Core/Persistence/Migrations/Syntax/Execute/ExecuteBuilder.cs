using Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute
{
    public class ExecuteBuilder : IExecuteBuilder
    {
        private readonly IMigrationContext _context;

        public ExecuteBuilder(IMigrationContext context)
        {
            _context = context;
        }

        public void Sql(string sqlStatement)
        {
            var expression = new ExecuteSqlStatementExpression {SqlStatement = sqlStatement};
            _context.Expressions.Add(expression);
        }
    }
}