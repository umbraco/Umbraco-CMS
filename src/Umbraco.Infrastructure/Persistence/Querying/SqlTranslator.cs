using NPoco;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

/// <summary>
///     Represents the Sql Translator for translating a IQuery object to Sql
/// </summary>
/// <typeparam name="T"></typeparam>
public class SqlTranslator<T>
{
    private readonly Sql<ISqlContext> _sql;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.Querying.SqlTranslator{T}"/> class.
    /// </summary>
    /// <param name="sql">The SQL context used for building and translating the query.</param>
    /// <param name="query">The query object to be translated into SQL.</param>
    public SqlTranslator(Sql<ISqlContext> sql, IQuery<T>? query)
    {
        _sql = sql ?? throw new ArgumentNullException(nameof(sql));
        if (query is not null)
        {
            foreach (Tuple<string, object[]> clause in query.GetWhereClauses())
            {
                _sql.Where(clause.Item1, clause.Item2);
            }
        }
    }

    /// <summary>
    /// Returns the underlying <see cref="Sql{ISqlContext}"/> object representing the translated SQL statement for the query.
    /// </summary>
    /// <returns>The <see cref="Sql{ISqlContext}"/> representing the translated SQL statement.</returns>
    public Sql<ISqlContext> Translate() => _sql;

    /// <summary>Returns the SQL query string represented by this translator.</summary>
    /// <returns>The SQL query string.</returns>
    public override string ToString() => _sql.SQL;
}
