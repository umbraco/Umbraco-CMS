using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Persistence.NPocoTests
{
    public class SqlTemplate
    {
        private static readonly Dictionary<string, SqlTemplate> Templates = new Dictionary<string, SqlTemplate>();
        public static SqlContext SqlContext; // FIXME must initialize somehow? OR have an easy access to templates through DatabaseContext?

        private readonly string _sql;
        private readonly Dictionary<int, string> _args;

        public SqlTemplate(string sql, object[] args)
        {
            _sql = sql;
            if (args.Length > 0)
                _args = new Dictionary<int, string>();
            for (var i = 0; i < args.Length; i++)
                _args[i] = args[i].ToString();
        }

        // for tests
        internal static void Clear()
        {
            Templates.Clear();
        }

        public static SqlTemplate Get(string key, Func<Sql<SqlContext>, Sql<SqlContext>> sqlBuilder)
        {
            if (Templates.TryGetValue(key, out var template)) return template;
            var sql = sqlBuilder(new Sql<SqlContext>(SqlContext));
            return Templates[key] = new SqlTemplate(sql.SQL, sql.Arguments);
        }

        public Sql<SqlContext> Sql()
        {
            return new Sql<SqlContext>(SqlContext, _sql);
        }

        // must pass the args in the proper order, faster
        public Sql<SqlContext> Sql(params object[] args)
        {
            return new Sql<SqlContext>(SqlContext, _sql, args);
        }

        // can pass named args, slower
        // so, not much different from what Where(...) does (ie reflection)
        public Sql<SqlContext> SqlNamed(object nargs)
        {
            var args = new object[_args.Count];
            var properties = nargs.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(nargs));
            for (var i = 0; i < _args.Count; i++)
            {
                if (!properties.TryGetValue(_args[i], out var value))
                    throw new InvalidOperationException($"Invalid argument name \"{_args[i]}\".");
                args[i] = value;
            }
            return new Sql<SqlContext>(SqlContext, _sql, args);
        }
    }
}