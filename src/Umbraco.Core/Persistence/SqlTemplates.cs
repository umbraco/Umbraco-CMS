using System;
using System.Collections.Concurrent;
using NPoco;

namespace Umbraco.Core.Persistence
{
    public class SqlTemplates
    {
        private readonly ConcurrentDictionary<string, SqlTemplate> _templates = new ConcurrentDictionary<string, SqlTemplate>();
        private readonly ISqlContext _sqlContext;

        public SqlTemplates(ISqlContext sqlContext)
        {
            _sqlContext = sqlContext;
        }

        // for tests
        internal void Clear()
        {
            _templates.Clear();
        }

        public SqlTemplate Get(string key, Func<Sql<ISqlContext>, Sql<ISqlContext>> sqlBuilder)
        {
            SqlTemplate CreateTemplate(string _)
            {
                var sql = sqlBuilder(new Sql<ISqlContext>(_sqlContext));
                return new SqlTemplate(_sqlContext, sql.SQL, sql.Arguments);
            }

            return _templates.GetOrAdd(key, CreateTemplate);
        }
    }
}
