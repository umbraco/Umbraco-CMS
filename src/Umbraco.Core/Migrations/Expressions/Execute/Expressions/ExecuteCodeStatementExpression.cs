using System;
using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Execute.Expressions
{
    public class ExecuteCodeStatementExpression : MigrationExpressionBase
    {
        public ExecuteCodeStatementExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public virtual Func<IMigrationContext, string> CodeStatement { get; set; }

        public override string Process(IMigrationContext context)
        {
            return CodeStatement != null ? CodeStatement(context) : base.Process(context);
        }
    }
}
