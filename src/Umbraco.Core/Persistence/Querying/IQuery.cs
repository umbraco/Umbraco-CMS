using System;
using System.Linq.Expressions;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Represents a query for building Linq translatable SQL queries
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQuery<T>
    {
        /// <summary>
        /// Adds a where clause to the query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>This instance so calls to this method are chainable</returns>
        IQuery<T> Where(Expression<Func<T, bool>> predicate);

        
    }
}