using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the UserRepository for doing CRUD operations for users
    /// </summary>
    internal class UserRepository : PetaPocoRepositoryBase<int, IUser>, IUserRepository
    {
        public UserRepository(IUnitOfWork work)
            : base(work)
        {
        }

        #region Overrides of RepositoryBase<int,IUser>

        protected override IUser PerformGet(int id)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IUser> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IUser> PerformGetByQuery(IQuery<IUser> query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IUser>

        protected override Sql GetBaseQuery(bool isCount)
        {
            throw new NotImplementedException();
        }

        protected override string GetBaseWhereClause()
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

        protected override void PersistNewItem(IUser entity)
        {
            throw new NotImplementedException();
        }

        protected override void PersistUpdatedItem(IUser entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}