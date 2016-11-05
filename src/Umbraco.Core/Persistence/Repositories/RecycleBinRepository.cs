using System.Collections.Generic;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal abstract class RecycleBinRepository<TId, TEntity, TRepository> : VersionableRepositoryBase<TId, TEntity, TRepository>, IRecycleBinRepository<TEntity> 
        where TEntity : class, IUmbracoEntity
        where TRepository :  class, IRepository
    {
        protected RecycleBinRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, IContentSection contentSection, IMapperCollection mappers)
            : base(work, cache, logger, contentSection, mappers)
        {
        }

        protected abstract int RecycleBinId { get; }

        public virtual IEnumerable<TEntity> GetEntitiesInRecycleBin()
        {
            return GetByQuery(Query.Where(entity => entity.Trashed));
        }
    }
}