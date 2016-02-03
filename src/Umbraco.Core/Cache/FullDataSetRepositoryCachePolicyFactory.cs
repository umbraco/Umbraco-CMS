using System;
using System.Collections.Generic;
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
        private readonly Func<IEnumerable<TEntity>> _getAllFromRepo;
        private readonly bool _expires;

        public FullDataSetRepositoryCachePolicyFactory(IRuntimeCacheProvider runtimeCache, Func<TEntity, TId> getEntityId, Func<IEnumerable<TEntity>> getAllFromRepo, bool expires)
        {
            _runtimeCache = runtimeCache;
            _getEntityId = getEntityId;
            _getAllFromRepo = getAllFromRepo;
            _expires = expires;
        }

        public virtual IRepositoryCachePolicy<TEntity, TId> CreatePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<TEntity, TId>(_runtimeCache, _getEntityId, _getAllFromRepo, _expires);
        }
    }
}