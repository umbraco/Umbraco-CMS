using System;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions
{
    public class ExecuteCodeStatementExpression : MigrationExpressionBase
    {
        public ExecuteCodeStatementExpression()
        {
        }

        public ExecuteCodeStatementExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders)
            : base(current, databaseProviders)
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