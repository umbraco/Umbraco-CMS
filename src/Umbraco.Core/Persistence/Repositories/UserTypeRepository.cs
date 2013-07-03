using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the UserTypeRepository for doing CRUD operations for <see cref="IUserType"/>
    /// </summary>
    internal class UserTypeRepository : PetaPocoRepositoryBase<int, IUserType>, IUserTypeRepository
    {
		public UserTypeRepository(IDatabaseUnitOfWork work)
			: base(work)
        {
        }

		public UserTypeRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
			: base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<int,IUserType>

        protected override IUserType PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.FirstOrDefault<UserTypeDto>(sql);

            if (dto == null)
                return null;

            var userTypeFactory = new UserTypeFactory();
            var userType = userTypeFactory.BuildEntity(dto);

            return userType;
        }

        protected override IEnumerable<IUserType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var userDtos = Database.Fetch<UserTypeDto>("WHERE id >= 0");
                foreach (var userDto in userDtos)
                {
                    yield return Get(userDto.Id);
                }
            }
        }

        protected override IEnumerable<IUserType> PerformGetByQuery(IQuery<IUserType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IUserType>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<UserTypeDto>(sql);

            foreach (var dto in dtos.DistinctBy(x => x.Id))
            {
                yield return Get(dto.Id);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IUserType>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From<UserTypeDto>();
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoUserType.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM umbracoUser WHERE userType = @Id",
                               "DELETE FROM umbracoUserType WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(IUserType entity)
        {
            var userTypeFactory = new UserTypeFactory();
            var userTypeDto = userTypeFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(userTypeDto));
            entity.Id = id;
        }

        protected override void PersistUpdatedItem(IUserType entity)
        {
            var userTypeFactory = new UserTypeFactory();
            var userTypeDto = userTypeFactory.BuildDto(entity);

            Database.Update(userTypeDto);
        }

        #endregion
    }
}