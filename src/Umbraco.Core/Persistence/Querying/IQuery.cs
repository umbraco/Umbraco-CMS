using System;
using System.Linq.Expressions;

namespace Umbraco.Core.Persistence.Querying
{
    public interface IQuery<T>
    {
        IQuery<T> Where(Expression<Func<T, bool>> predicate);
    }
}