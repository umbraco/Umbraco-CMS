using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IEntityRepository : IRepository
    {
        IEntitySlim Get(int id);
        IEntitySlim Get(Guid key);
        IEntitySlim Get(int id, Guid objectTypeId);
        IEntitySlim Get(Guid key, Guid objectTypeId);

        IEnumerable<IEntitySlim> GetAll(Guid objectType, params int[] ids);
        IEnumerable<IEntitySlim> GetAll(Guid objectType, params Guid[] keys);

        IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query);
        IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectType);

        UmbracoObjectTypes GetObjectType(int id);
        UmbracoObjectTypes GetObjectType(Guid key);

        IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params int[] ids);
        IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params Guid[] keys);

        bool Exists(int id);
        bool Exists(Guid key);

        IEnumerable<IEntitySlim> GetPagedResultsByQuery(IQuery<IUmbracoEntity> query, Guid objectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, IQuery<IUmbracoEntity> filter = null);
    }
}
