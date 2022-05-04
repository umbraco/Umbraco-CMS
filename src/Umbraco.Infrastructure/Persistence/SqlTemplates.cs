using System.Collections.Concurrent;
using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence;

public class SqlTemplates
{
    private readonly ISqlContext _sqlContext;
    private readonly ConcurrentDictionary<string, SqlTemplate> _templates = new();

    public SqlTemplates(ISqlContext sqlContext) => _sqlContext = sqlContext;

    public SqlTemplate Get(string key, Func<Sql<ISqlContext>, Sql<ISqlContext>> sqlBuilder)
    {
        SqlTemplate CreateTemplate(string _)
        {
            Sql<ISqlContext> sql = sqlBuilder(new Sql<ISqlContext>(_sqlContext));
            return new SqlTemplate(_sqlContext, sql.SQL, sql.Arguments);
        }

        return _templates.GetOrAdd(key, CreateTemplate);
    }

    // for tests
    internal void Clear() => _templates.Clear();
}
