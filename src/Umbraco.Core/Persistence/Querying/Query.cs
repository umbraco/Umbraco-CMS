using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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
                //TODO: This should have an SqlSyntax object passed in, this ctor is relying on a singleton
                var expressionHelper = new ModelToSqlExpressionVisitor<T>();
                string whereExpression = expressionHelper.Visit(predicate);

                _wheres.Add(new Tuple<string, object[]>(whereExpression, expressionHelper.GetSqlParameters()));
            }
            return this;
        }

        /// <summary>
        /// Adds a set of OR-ed where clauses to the query.
        /// </summary>
        /// <param name="predicates"></param>
        /// <returns>This instance so calls to this method are chainable.</returns>
        public virtual IQuery<T> WhereAny(IEnumerable<Expression<Func<T, bool>>> predicates)
        {
            if (predicates == null) return this;

            StringBuilder sb = null;
            List<object> parameters = null;
            Sql sql = null;
            foreach (var predicate in predicates)
            {
                // see notes in Where()
                var expressionHelper = new ModelToSqlExpressionVisitor<T>();
                var whereExpression = expressionHelper.Visit(predicate);

                if (sb == null)
                {
                    sb = new StringBuilder("(");
                    parameters = new List<object>();
                    sql = new Sql();
                }
                else
                {
                    sb.Append(" OR ");
                    sql.Append(" OR ");
                }

                sb.Append(whereExpression);
                parameters.AddRange(expressionHelper.GetSqlParameters());
                sql.Append(whereExpression, expressionHelper.GetSqlParameters());
            }

            if (sb == null) return this;

            sb.Append(")");
            //_wheres.Add(Tuple.Create(sb.ToString(), parameters.ToArray()));
            _wheres.Add(Tuple.Create("(" + sql.SQL + ")", sql.Arguments));

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