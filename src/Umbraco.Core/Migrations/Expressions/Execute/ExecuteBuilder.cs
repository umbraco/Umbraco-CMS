using System;
using NPoco;
using Umbraco.Core.Migrations.Expressions.Execute.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Execute
{
    /// <summary>
    /// Implements <see cref="IExecuteBuilder"/>.
    /// </summary>
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
            expression.Execute();
        }

        public void Code(Func<IMigrationContext, string> codeStatement)
        {
            var expression = new ExecuteCodeStatementExpression(_context, _supportedDatabaseTypes) { CodeStatement = codeStatement };
            _context.Expressions.Add(expression);
        }
    }
}
