using System;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions
{
    public class ExecuteCodeStatementExpression : MigrationExpressionBase
    {
        public ExecuteCodeStatementExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(current, databaseProviders, sqlSyntax)
        {
        }

        public virtual Func<Database, string> CodeStatement { get; set; }

        public override string Process(Database database)
        {
            if(CodeStatement != null)
                return CodeStatement(database);

            return base.Process(database);
        }
    }
}