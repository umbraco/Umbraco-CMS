using System;
using NPoco;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions
{
    public class ExecuteCodeStatementExpression : MigrationExpressionBase
    {
        public ExecuteCodeStatementExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public virtual Func<IUmbracoDatabase, string> CodeStatement { get; set; }

        public override string Process(IUmbracoDatabase database)
        {
            if(CodeStatement != null)
                return CodeStatement(database);

            return base.Process(database);
        }
    }
}
