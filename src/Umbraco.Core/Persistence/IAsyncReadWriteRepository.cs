namespace Umbraco.Cms.Core.Persistence;

public interface IAsyncReadWriteRepository<in TKey, TEntity> : IAsyncReadRepository<TKey, TEntity>, IAsyncWriteRepository<TEntity>
{
}
