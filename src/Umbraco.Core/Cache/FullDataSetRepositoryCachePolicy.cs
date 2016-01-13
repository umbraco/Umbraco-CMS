using System.Linq;
using Umbraco.Core.Collections;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A caching policy that caches an entire dataset as a single collection
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    internal class FullDataSetRepositoryCachePolicy<TEntity, TId> : DefaultRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        public FullDataSetRepositoryCachePolicy(IRuntimeCacheProvider cache) : base(cache, new RepositoryCachePolicyOptions())
        {
        }

        /// <summary>
        /// For this type of caching policy, we don't cache individual items
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="entity"></param>
        protected override void SetCacheAction(string cacheKey, TEntity entity)
        {
            //do nothing
        }

        /// <summary>
        /// Sets the action to execute on disposal for an entity collection
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="entityCollection"></param>
        protected override void SetCacheAction(TId[] ids, TEntity[] entityCollection)
        {
            //for this type of caching policy, we don't want to cache any GetAll request containing specific Ids
            if (ids.Any()) return;

            //set the disposal action
            SetCacheAction(() =>
            {
                //We want to cache the result as a single collection
                Cache.InsertCacheItem(GetCacheTypeKey(), () => new DeepCloneableList<TEntity>(entityCollection));
            });
        }

        /// <summary>
        /// This policy will cache the full data set as a single collection
        /// </summary>
        /// <returns></returns>
        protected override TEntity[] GetAllFromCache()
        {
            var found = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetCacheTypeKey());
            return found == null ? new TEntity[] { } : found.WhereNotNull().ToArray();
        }
    }
}