namespace Umbraco.Cms.Core.Persistence;

public interface IAsyncReadWriteRepository<in TId, TEntity> : IAsyncReadRepository<TId, TEntity>, IAsyncWriteRepository<TEntity>
{
}
