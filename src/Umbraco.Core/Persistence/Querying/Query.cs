using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using System.Text;
using NPoco;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Represents the Query Builder for building LINQ translatable queries
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Query<T> : IQuery<T>
    {
        private readonly SqlContext _sqlContext;
        private readonly List<Tuple<string, object[]>> _wheres = new List<Tuple<string, object[]>>();

        public Query(SqlContext sqlContext)
        {
            _sqlContext = sqlContext;
        }

        /// <summary>
        /// Adds a where clause to the query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>This instance so calls to this method are chainable</returns>
        public virtual IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) return this;

            var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlContext.SqlSyntax, _sqlContext.Mappers);
            var whereExpression = expressionHelper.Visit(predicate);
            _wheres.Add(new Tuple<string, object[]>(whereExpression, expressionHelper.GetSqlParameters()));
            return this;
        }

        public virtual IQuery<T> WhereIn(Expression<Func<T, object>> fieldSelector, IEnumerable values)
        {
            if (fieldSelector == null) return this;

            var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlContext.SqlSyntax, _sqlContext.Mappers);
            var whereExpression = expressionHelper.Visit(fieldSelector);
            _wheres.Add(new Tuple<string, object[]>(whereExpression + " IN (@values)", new object[] { new { values } }));
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
            Sql<SqlContext> sql = null;
            foreach (var predicate in predicates)
            {
                // see notes in Where()
                var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlContext.SqlSyntax, _sqlContext.Mappers);
                var whereExpression = expressionHelper.Visit(predicate);

                if (sb == null)
                {
                    sb = new StringBuilder("(");
                    parameters = new List<object>();
                    sql = Sql.BuilderFor(_sqlContext);
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
    }
}
