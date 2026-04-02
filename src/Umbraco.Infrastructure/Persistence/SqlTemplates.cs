using System.Collections.Concurrent;
using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Provides a collection of predefined SQL templates used for various persistence operations within the Umbraco CMS infrastructure layer.
/// This class centralizes SQL statements to promote reuse and maintainability.
/// </summary>
public class SqlTemplates
{
    private readonly ISqlContext _sqlContext;
    private readonly ConcurrentDictionary<string, SqlTemplate> _templates = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTemplates"/> class with the specified SQL context.
    /// </summary>
    /// <param name="sqlContext">The <see cref="ISqlContext"/> used for SQL template generation.</param>
    public SqlTemplates(ISqlContext sqlContext) => _sqlContext = sqlContext;

    /// <summary>
    /// Gets a <see cref="SqlTemplate"/> by key, creating it using the provided SQL builder function if it does not exist.
    /// </summary>
    /// <param name="key">The key identifying the SQL template.</param>
    /// <param name="sqlBuilder">A function that builds the SQL query given a <see cref="Sql{ISqlContext}"/> instance.</param>
    /// <returns>The <see cref="SqlTemplate"/> associated with the specified key.</returns>
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
