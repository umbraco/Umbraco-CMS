using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Represents the Query Builder for building LINQ translatable queries
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Query<T> : IQuery<T>
    {
        private readonly List<Tuple<string, object[]>> _wheres = new List<Tuple<string, object[]>>();

        /// <summary>
        /// Helper method to be used instead of manually creating an instance
        /// </summary>
        public static IQuery<T> Builder
        {
            get { return new Query<T>(); }
        }

        /// <summary>
        /// Adds a where clause to the query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>This instance so calls to this method are chainable</returns>
        public virtual IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                var expressionHelper = new ModelToSqlExpressionHelper<T>();
                string whereExpression = expressionHelper.Visit(predicate);

                _wheres.Add(new Tuple<string, object[]>(whereExpression, expressionHelper.GetSqlParameters()));
            }
            return this;
        }

        /// <summary>
        /// Returns all translated where clauses and their sql parameters
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<string, object[]>> GetWhereClauses()
        {
            return _wheres;
        }

        [Obsolete("This is no longer used, use the GetWhereClauses method which includes the SQL parameters")]
        public List<string> WhereClauses()
        {
            return _wheres.Select(x => x.Item1).ToList();
        }
    }
}