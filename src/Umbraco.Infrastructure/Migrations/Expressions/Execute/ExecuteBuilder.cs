using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute;

public class ExecuteBuilder : ExpressionBuilderBase<ExecuteSqlStatementExpression>,
    IExecuteBuilder, IExecutableBuilder
{
    public ExecuteBuilder(IMigrationContext context)
        : base(new ExecuteSqlStatementExpression(context))
    {
    }

    /// <inheritdoc />
    public void Do()
    {
        // slightly awkward, but doing it right would mean a *lot*
        // of changes for MigrationExpressionBase
        if (Expression.SqlObject == null)
        {
            Expression.Execute();
        }
        else
        {
            Expression.ExecuteSqlObject();
        }
    }

    /// <inheritdoc />
    public IExecutableBuilder Sql(string sqlStatement)
    {
        Expression.SqlStatement = sqlStatement;
        return this;
    }

    /// <inheritdoc />
    public IExecutableBuilder Sql(Sql<ISqlContext> sql)
    {
        Expression.SqlObject = sql;
        return this;
    }
}
