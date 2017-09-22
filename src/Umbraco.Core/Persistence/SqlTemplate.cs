using System;
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
            return new Sql<ISqlContext>(_sqlContext, _sql, args);
        }

        // can pass named args, slower
        // so, not much different from what Where(...) does (ie reflection)
        public Sql<ISqlContext> SqlNamed(object nargs)
        {
            var args = new object[_args.Count];
            var properties = nargs.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(nargs));
            for (var i = 0; i < _args.Count; i++)
            {
                if (!properties.TryGetValue(_args[i], out var value))
                    throw new InvalidOperationException($"Invalid argument name \"{_args[i]}\".");
                args[i] = value;
            }
            return new Sql<ISqlContext>(_sqlContext, _sql, args);
        }
    }
}