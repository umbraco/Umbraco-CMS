using NPoco;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using IMapperCollection = Umbraco.Cms.Infrastructure.Persistence.Mappers.IMapperCollection;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Specifies the Sql context.
/// </summary>
public interface ISqlContext
{
    /// <summary>
    ///     Gets the Sql syntax provider.
    /// </summary>
    ISqlSyntaxProvider SqlSyntax { get; }

    /// <summary>
    ///     Gets the database type.
    /// </summary>
    DatabaseType DatabaseType { get; }

    /// <summary>
    ///     Gets the Sql templates.
    /// </summary>
    SqlTemplates Templates { get; }

    /// <summary>
    ///     Gets the Poco data factory.
    /// </summary>
    IPocoDataFactory PocoDataFactory { get; }

    /// <summary>
    ///     Gets the mappers.
    /// </summary>
    IMapperCollection? Mappers { get; }

    /// <summary>
    ///     Creates a new Sql expression.
    /// </summary>
    /// <returns>A new Sql expression.</returns>
    Sql<ISqlContext> Sql();

    /// <summary>
    ///     Creates a new <see cref="Sql{ISqlContext}"/> expression from the specified SQL string and arguments.
    /// </summary>
    /// <param name="sql">The SQL query string to create the expression from.</param>
    /// <param name="args">The arguments to be used in the SQL query string.</param>
    /// <returns>A new <see cref="Sql{ISqlContext}"/> expression representing the specified SQL query.</returns>
    Sql<ISqlContext> Sql(string sql, params object[] args);

    /// <summary>
    ///     Creates a new query expression for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the entity to query.</typeparam>
    /// <returns>A new query expression of type <see cref="IQuery{T}"/>.</returns>
    IQuery<T> Query<T>();
}
