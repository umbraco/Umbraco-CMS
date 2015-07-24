using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class ExternalLoginRepository : PetaPocoRepositoryBase<int, IIdentityUserLogin>, IExternalLoginRepository
    {
        public ExternalLoginRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        public void DeleteUserLogins(int memberId)
        {
            using (var t = Database.GetTransaction())
            {
                Database.Execute("DELETE FROM ExternalLogins WHERE UserId=@userId", new { userId = memberId });

                t.Complete();
            }
        }

        public void SaveUserLogins(int memberId, IEnumerable<UserLoginInfo> logins)
        {
            using (var t = Database.GetTransaction())
            {
                //clear out logins for member
                Database.Execute("DELETE FROM umbracoExternalLogin WHERE userId=@userId", new { userId = memberId });

                //add them all
                foreach (var l in logins)
                {
                    Database.Insert(new ExternalLoginDto
                    {
                        LoginProvider = l.LoginProvider,
                        ProviderKey = l.ProviderKey,
                        UserId = memberId,
                        CreateDate = DateTime.Now
                    });
                }

                t.Complete();
            }
        }

        protected override IIdentityUserLogin PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var macroDto = Database.Fetch<ExternalLoginDto>(sql).FirstOrDefault();
            if (macroDto == null)
                return null;

            var factory = new ExternalLoginFactory();
            var entity = factory.BuildEntity(macroDto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<IIdentityUserLogin> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                return PerformGetAllOnIds(ids);
            }

            var sql = GetBaseQuery(false);

            return ConvertFromDtos(Database.Fetch<ExternalLoginDto>(sql))
                .ToArray();// we don't want to re-iterate again!
        }

        private IEnumerable<IIdentityUserLogin> PerformGetAllOnIds(params int[] ids)
        {
            if (ids.Any() == false) yield break;
            foreach (var id in ids)
            {
                yield return Get(id);
            }
        }

        private IEnumerable<IIdentityUserLogin> ConvertFromDtos(IEnumerable<ExternalLoginDto> dtos)
        {
            var factory = new ExternalLoginFactory();
            foreach (var entity in dtos.Select(factory.BuildEntity))
            {
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);

                yield return entity;
            }
        }

        protected override IEnumerable<IIdentityUserLogin> PerformGetByQuery(IQuery<IIdentityUserLogin> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IIdentityUserLogin>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<ExternalLoginDto>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.Id);
            }
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            if (isCount)
            {
                sql.Select("COUNT(*)").From<ExternalLoginDto>(SqlSyntax);
            }
            else
            {
                sql.Select("*").From<ExternalLoginDto>(SqlSyntax);
            }
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoExternalLogin.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM umbracoExternalLogin WHERE id = @Id"                           
                };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(IIdentityUserLogin entity)
        {
            ((Entity)entity).AddingEntity();

            var factory = new ExternalLoginFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IIdentityUserLogin entity)
        {
            ((Entity)entity).UpdatingEntity();

            var factory = new ExternalLoginFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);          

            entity.ResetDirtyProperties();
        }
    }
}