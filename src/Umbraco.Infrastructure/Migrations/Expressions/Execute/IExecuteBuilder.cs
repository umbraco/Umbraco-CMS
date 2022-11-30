using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute;

/// <summary>
///     Builds and executes an Sql statement.
/// </summary>
/// <remarks>Deals with multi-statements Sql.</remarks>
public interface IExecuteBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the Sql statement to execute.
    /// </summary>
    IExecutableBuilder Sql(string sqlStatement);

    /// <summary>
    ///     Specifies the Sql statement to execute.
    /// </summary>
    IExecutableBuilder Sql(Sql<ISqlContext> sql);
}
