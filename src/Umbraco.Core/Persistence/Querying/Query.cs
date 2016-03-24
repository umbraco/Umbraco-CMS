using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Core.Models.PublishedContent;
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
        private readonly IMappingResolver _mappingResolver;
        private readonly List<Tuple<string, object[]>> _wheres = new List<Tuple<string, object[]>>();

        public Query(ISqlSyntaxProvider sqlSyntax, IMappingResolver mappingResolver)
        {
            _sqlSyntax = sqlSyntax;
            _mappingResolver = mappingResolver;
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
                var expressionHelper = new ModelToSqlExpressionHelper<T>(_sqlSyntax, _mappingResolver);
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

    }
}