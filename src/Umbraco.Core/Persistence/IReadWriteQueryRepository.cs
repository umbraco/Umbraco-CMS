namespace Umbraco.Cms.Core.Persistence;

/// <summary>
///     Defines the base implementation of a reading, writing and querying repository.
/// </summary>
public interface IReadWriteQueryRepository<in TId, TEntity> : IReadRepository<TId, TEntity>, IWriteRepository<TEntity>,
    IQueryRepository<TEntity>
{
}
