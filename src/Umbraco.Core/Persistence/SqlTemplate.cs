using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NPoco;

namespace Umbraco.Core.Persistence
{
    public class SqlTemplate
    {
        private readonly ISqlContext _sqlContext;
        private readonly string _sql;
        private readonly Dictionary<int, string> _args;

        internal SqlTemplate(ISqlContext sqlContext, string sql, object[] args)
        {
            _sqlContext = sqlContext;
            _sql = sql;
            if (args.Length > 0)
                _args = new Dictionary<int, string>();
            for (var i = 0; i < args.Length; i++)
                _args[i] = args[i].ToString();
        }

        public Sql<ISqlContext> Sql()
        {
            return new Sql<ISqlContext>(_sqlContext, _sql);
        }

        // must pass the args in the proper order, faster
        public Sql<ISqlContext> Sql(params object[] args)
        {
            // if the type is an "unspeakable name" it is an anonymous compiler-generated object
            // see https://stackoverflow.com/questions/9256594
            // => assume it's an anonymous type object containing named arguments
            // (of course this means we cannot use *real* objects here and need SqlNamed - bah)
            if (args.Length == 1 && args[0].GetType().Name.Contains("<"))
                return SqlNamed(args[0]);

            if (args.Length != _args.Count)
                throw new ArgumentException("Invalid number of arguments.", nameof(args));

            if (args.Length == 0)
                return new Sql<ISqlContext>(_sqlContext, true, _sql);

            var isBuilt = !args.Any(x => x is IEnumerable);
            return new Sql<ISqlContext>(_sqlContext, isBuilt, _sql, args);
        }

        // can pass named args, slower
        // so, not much different from what Where(...) does (ie reflection)
        public Sql<ISqlContext> SqlNamed(object nargs)
        {
            var isBuilt = true;
            var args = new object[_args.Count];
            var properties = nargs.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(nargs));
            for (var i = 0; i < _args.Count; i++)
            {
                if (!properties.TryGetValue(_args[i], out var value))
                    throw new InvalidOperationException($"Missing argument \"{_args[i]}\".");
                args[i] = value;
                properties.Remove(_args[i]);

                // if value is enumerable then we'll need to expand arguments
                if (value is IEnumerable)
                    isBuilt = false;
            }
            if (properties.Count > 0)
                throw new InvalidOperationException($"Unknown argument{(properties.Count > 1 ? "s" : "")}: {string.Join(", ", properties.Keys)}");
            return new Sql<ISqlContext>(_sqlContext, isBuilt, _sql, args);
        }

        internal void WriteToConsole()
        {
            new Sql<ISqlContext>(_sqlContext, _sql, _args.Values.Cast<object>().ToArray()).WriteToConsole();
        }

        public static T Arg<T>(string name)
        {
            return default (T);
        }

        public static IEnumerable<T> ArgIn<T>(string name)
        {
            // don't return an empty enumerable, as it breaks NPoco
            // fixme - should we cache these arrays?
            return new[] { default (T) };
        }
    }
}
