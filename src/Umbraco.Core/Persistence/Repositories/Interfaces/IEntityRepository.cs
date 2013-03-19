using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IEntityRepository : IRepository
    {
        IUmbracoEntity Get(int id);
        IUmbracoEntity Get(int id, Guid objectTypeId);
        IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId, params int[] ids);
        IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query);
        IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectTypeId);
    }
}