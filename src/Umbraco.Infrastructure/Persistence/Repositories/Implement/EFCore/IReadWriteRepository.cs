using Umbraco.Cms.Core.Persistence;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

public interface IReadWriteRepository<in TId, TEntity> : IReadRepository<TId, TEntity>, IWriteRepository<TEntity>
{
}
