using System;
using System.Collections.Generic;
using NPoco;

namespace Umbraco.Core.Persistence
{
    public class SqlTemplates
    {
        private readonly Dictionary<string, SqlTemplate> _templates = new Dictionary<string, SqlTemplate>();
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
            if (_templates.TryGetValue(key, out var template)) return template;
            var sql = sqlBuilder(new Sql<ISqlContext>(_sqlContext));
            return _templates[key] = new SqlTemplate(_sqlContext, sql.SQL, sql.Arguments);
        }
    }
}
