using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Creates cache policies
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    internal class DefaultRepositoryCachePolicyFactory<TEntity, TId> : IRepositoryCachePolicyFactory<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly RepositoryCachePolicyOptions _options;

        public DefaultRepositoryCachePolicyFactory(IRuntimeCacheProvider runtimeCache, RepositoryCachePolicyOptions options)
        {
            _runtimeCache = runtimeCache;
            _options = options;
        }

        public virtual IRepositoryCachePolicy<TEntity, TId> CreatePolicy()
        {
            return new DefaultRepositoryCachePolicy<TEntity, TId>(_runtimeCache, _options);
        }
    }
}