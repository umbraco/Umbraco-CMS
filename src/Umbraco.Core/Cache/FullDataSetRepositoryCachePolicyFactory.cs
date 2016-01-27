using System;
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
        private readonly Func<TEntity, TId> _getEntityId;

        public FullDataSetRepositoryCachePolicyFactory(IRuntimeCacheProvider runtimeCache, Func<TEntity, TId> getEntityId)
        {
            _runtimeCache = runtimeCache;
            _getEntityId = getEntityId;
        }

        public virtual IRepositoryCachePolicy<TEntity, TId> CreatePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<TEntity, TId>(_runtimeCache, _getEntityId);
        }
    }
}