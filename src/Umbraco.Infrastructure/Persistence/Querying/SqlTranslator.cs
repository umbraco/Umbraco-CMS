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

    public Sql<ISqlContext> Translate() => _sql;

    public override string ToString() => _sql.SQL;
}
