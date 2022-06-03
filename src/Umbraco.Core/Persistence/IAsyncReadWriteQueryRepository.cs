namespace Umbraco.Cms.Core.Persistence
{
    /// <summary>
    /// Defines the base implementation of a reading, writing and querying repository.
    /// </summary>
    public interface IAsyncReadWriteQueryRepository<in TId, TEntity> : IAsyncReadRepository<TId, TEntity>, IReadWriteQueryRepository<TId,TEntity>
    { }
}
