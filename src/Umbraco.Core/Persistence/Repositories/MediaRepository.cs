using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMedia"/>
    /// </summary>
    internal class MediaRepository : PetaPocoRepositoryBase<int, IMedia>, IMediaRepository
    {
        public MediaRepository(IUnitOfWork work) : base(work)
        {
        }

        public MediaRepository(IUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<int,IMedia>

        protected override IMedia PerformGet(int id)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IMedia> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IMedia> PerformGetByQuery(IQuery<IMedia> query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IMedia>

        protected override Sql GetBaseQuery(bool isCount)
        {
            throw new NotImplementedException();
        }

        protected override Sql GetBaseWhereClause(object id)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotImplementedException();
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Unit of Work Implementation
        
        protected override void PersistNewItem(IMedia entity)
        {
            throw new NotImplementedException();
        }

        protected override void PersistUpdatedItem(IMedia entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}