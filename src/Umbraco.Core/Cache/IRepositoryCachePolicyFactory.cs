using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    internal interface IRepositoryCachePolicyFactory<TEntity, TId> where TEntity : class, IAggregateRoot
    {
        IRepositoryCachePolicy<TEntity, TId> CreatePolicy();
    }
}