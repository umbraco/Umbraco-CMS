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
    /// Specifies the SQL statement to be executed as part of the migration.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement to execute.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> that can be used to continue building the execution.</returns>
    IExecutableBuilder Sql(string sqlStatement);

    /// <summary>
    /// Specifies the SQL statement to execute as part of a migration.
    /// </summary>
    /// <param name="sql">The SQL statement to execute.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> that allows further configuration of the migration step.</returns>
    IExecutableBuilder Sql(Sql<ISqlContext> sql);
}
