using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the UserTypeRepository for doing CRUD operations for <see cref="IUserType"/>
    /// </summary>
    internal class UserTypeRepository : PetaPocoRepositoryBase<int, IUserType>, IUserTypeRepository
    {
        public UserTypeRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        #region Overrides of RepositoryBase<int,IUserType>

        protected override IUserType PerformGet(int id)
        {
            return GetAll(new[] {id}).FirstOrDefault();
        }

        protected override IEnumerable<IUserType> PerformGetAll(params int[] ids)
        {
            var userTypeFactory = new UserTypeFactory();

            var sql = GetBaseQuery(false);
            
            if (ids.Any())
            {
                sql.Where("umbracoUserType.id in (@ids)", new { ids = ids });
            }
            else
            {
                sql.Where<UserTypeDto>(x => x.Id >= 0);
            }

            var dtos = Database.Fetch<UserTypeDto>(sql);
            return dtos.Select(userTypeFactory.BuildEntity).ToArray();
        }

        protected override IEnumerable<IUserType> PerformGetByQuery(IQuery<IUserType> query)
        {
            var userTypeFactory = new UserTypeFactory();
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IUserType>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<UserTypeDto>(sql);

            return dtos.Select(userTypeFactory.BuildEntity).ToArray();
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