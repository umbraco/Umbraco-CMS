using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IContentTypeCompositionRepository<TEntity> : IRepositoryQueryable<int, TEntity>, IReadRepository<Guid, TEntity>
        where TEntity : IContentTypeComposition
    {
        TEntity Get(string alias);
    }
}