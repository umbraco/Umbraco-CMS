using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute;

/// <summary>
/// Provides a builder for constructing and executing database commands as part of a migration.
/// </summary>
public class ExecuteBuilder : ExpressionBuilderBase<ExecuteSqlStatementExpression>,
    IExecuteBuilder, IExecutableBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.ExecuteBuilder"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for this builder.</param>
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
