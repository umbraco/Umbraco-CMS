using Umbraco.Cms.Core.Persistence;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

public interface IAsyncReadWriteRepository<in TId, TEntity> : IAsyncReadRepository<TId, TEntity>, IAsyncWriteRepository<TEntity>
{
}
