using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Represents the Query Builder for building LINQ translatable queries
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Query<T> : IQuery<T>
    {
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly IMapperCollection _mappers;
        private readonly List<Tuple<string, object[]>> _wheres = new List<Tuple<string, object[]>>();

        public Query(ISqlSyntaxProvider sqlSyntax, IMapperCollection mappers)
        {
            _sqlSyntax = sqlSyntax;
            _mappers = mappers;
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
                var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlSyntax, _mappers);
                string whereExpression = expressionHelper.Visit(predicate);

                _wheres.Add(new Tuple<string, object[]>(whereExpression, expressionHelper.GetSqlParameters()));
            }
            return this;
        }

        public virtual IQuery<T> WhereIn(Expression<Func<T, object>> fieldSelector, IEnumerable values)
        {
            if (fieldSelector != null)
            {
                var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlSyntax, _mappers);
                string whereExpression = expressionHelper.Visit(fieldSelector);

                _wheres.Add(new Tuple<string, object[]>(whereExpression + " IN (@values)", new object[] { new { @values = values } }));
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
    }
}