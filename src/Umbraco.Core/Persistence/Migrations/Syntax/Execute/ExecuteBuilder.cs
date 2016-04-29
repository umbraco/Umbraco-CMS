using System;
using NPoco;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute
{
    public class ExecuteBuilder : IExecuteBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public ExecuteBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public void Sql(string sqlStatement)
        {
            var expression = new ExecuteSqlStatementExpression(_context, _supportedDatabaseTypes) {SqlStatement = sqlStatement};
            _context.Expressions.Add(expression);
        }

        public void Code(Func<Database, string> codeStatement)
        {
            var expression = new ExecuteCodeStatementExpression(_context, _supportedDatabaseTypes) { CodeStatement = codeStatement };
            _context.Expressions.Add(expression);
        }
    }
}