using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Creates cache policies
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    internal class FullDataSetRepositoryCachePolicyFactory<TEntity, TId> : IRepositoryCachePolicyFactory<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private readonly IRuntimeCacheProvider _runtimeCache;
        
        public FullDataSetRepositoryCachePolicyFactory(IRuntimeCacheProvider runtimeCache)
        {
            _runtimeCache = runtimeCache;
        }

        public virtual IRepositoryCachePolicy<TEntity, TId> CreatePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<TEntity, TId>(_runtimeCache);
        }
    }
}