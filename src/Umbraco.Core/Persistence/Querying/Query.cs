using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Umbraco.Core.Persistence.Querying
{
    public class Query<T> : IQuery<T>
    {
        private readonly ExpressionHelper<T> _expresionist = new ExpressionHelper<T>();
        private readonly List<string> _wheres = new List<string>();

        public Query()
            : base()
        {

        }

        public static IQuery<T> Builder
        {
            get
            {
                return new Query<T>();
            }
        }

        public virtual IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                string whereExpression = _expresionist.Visit(predicate);
                _wheres.Add(whereExpression);
            }
            return this;
        }

        public List<string> WhereClauses()
        {
            return _wheres;
        }
    }
}