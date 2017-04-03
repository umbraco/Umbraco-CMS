using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Represents a special policy that does not cache the result of GetAll.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <remarks>
    /// <para>Overrides the default repository cache policy and does not writes the result of GetAll
    /// to cache, but only the result of individual Gets. It does read the cache for GetAll, though.</para>
    /// <para>Used by DictionaryRepository.</para>
    /// </remarks>
    internal class SingleItemsOnlyRepositoryCachePolicy<TEntity, TId> : DefaultRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        public SingleItemsOnlyRepositoryCachePolicy(IRuntimeCacheProvider cache, RepositoryCachePolicyOptions options)
            : base(cache, options)
        { }

        protected override void InsertEntities(TId[] ids, TEntity[] entities)
        {
            // nop
        }
    }
}