using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using NUnit.Framework;

namespace Umbraco.Tests.Persistence.NPocoTests
{
    [TestFixture]
    public class NPocoSqlCacheTests
    {
        [Test]
        public void TestSqlTemplates()
        {
            // this can be used for queries that we know we'll use a *lot* and
            // want to cache as a (static) template for ever, and ever - note
            // that using a MemoryCache would allow us to set a size limit, or
            // something equivalent, to reduce risk of memory explosion
            var sql = SqlTemplate.Get("xxx", () => new Sql()
                .Select("*")
                .From("zbThing1")
                .Where("id=@id", new { id = "id" })).WithNamed(new { id = 1 });

            WriteSql(sql);

            var sql2 = SqlTemplate.Get("xxx", () => throw new InvalidOperationException("Should be cached.")).With(1);

            WriteSql(sql2);

            var sql3 = SqlTemplate.Get("xxx", () => throw new InvalidOperationException("Should be cached.")).WithNamed(new { id = 1 });

            WriteSql(sql3);
        }

        public class SqlTemplate
        {
            private static readonly Dictionary<string, SqlTemplate> Templates = new Dictionary<string, SqlTemplate>();

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

            public static SqlTemplate Get(string key, Func<Sql> sqlBuilder)
            {
                if (Templates.TryGetValue(key, out var template)) return template;
                var sql = sqlBuilder();
                return Templates[key] = new SqlTemplate(sql.SQL, sql.Arguments);
            }

            // must pass the args in the proper order, faster
            public Sql With(params object[] args)
            {
                return new Sql(_sql, args);
            }

            // can pass named args, slower
            // so, not much different from what Where(...) does (ie reflection)
            public Sql WithNamed(object nargs)
            {
                var args = new object[_args.Count];
                var properties = nargs.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
                for (var i = 0; i < _args.Count; i++)
                {
                    if (!properties.TryGetValue(_args[i], out var propertyInfo))
                        throw new InvalidOperationException($"Invalid argument name \"{_args[i]}\".");
                    args[i] = propertyInfo.GetValue(nargs);
                }
                return new Sql(_sql, args);
            }
        }

        private static void WriteSql(Sql sql)
        {
            Console.WriteLine();
            Console.WriteLine(sql.SQL);
            var i = 0;
            foreach (var arg in sql.Arguments)
                Console.WriteLine($"  @{i++}: {arg}");
        }
    }
}
