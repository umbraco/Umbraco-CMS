using System;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute
{
    public class ExecuteBuilder : IExecuteBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public ExecuteBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }

        public void Sql(string sqlStatement)
        {
            var expression = new ExecuteSqlStatementExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) {SqlStatement = sqlStatement};
            _context.Expressions.Add(expression);
        }

        public void Code(Func<Database, string> codeStatement)
        {
            var expression = new ExecuteCodeStatementExpression(_context.CurrentDatabaseProvider, _databaseProviders, _sqlSyntax) { CodeStatement = codeStatement };
            _context.Expressions.Add(expression);
        }
    }
}