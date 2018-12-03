using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class ExternalLoginRepository : NPocoRepositoryBase<int, IIdentityUserLogin>, IExternalLoginRepository
    {
        public ExternalLoginRepository(IScopeAccessor scopeAccessor, CacheHelper cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        public void DeleteUserLogins(int memberId)
        {
            Database.Execute("DELETE FROM ExternalLogins WHERE UserId=@userId", new { userId = memberId });
        }

        public void SaveUserLogins(int memberId, IEnumerable<UserLoginInfo> logins)
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
        }

        protected override IIdentityUserLogin PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.Fetch<ExternalLoginDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
            if (dto == null)
                return null;

            var entity = ExternalLoginFactory.BuildEntity(dto);

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);

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
            foreach (var entity in dtos.Select(ExternalLoginFactory.BuildEntity))
            {
                // reset dirty initial properties (U4-1946)
                ((BeingDirtyBase)entity).ResetDirtyProperties(false);

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

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();
            if (isCount)
                sql.SelectCount();
            else
                sql.SelectAll();
            sql.From<ExternalLoginDto>();
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoExternalLogin.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM umbracoExternalLogin WHERE id = @id"
                };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(IIdentityUserLogin entity)
        {
            ((EntityBase)entity).AddingEntity();

            var dto = ExternalLoginFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IIdentityUserLogin entity)
        {
            ((EntityBase)entity).UpdatingEntity();
            
            var dto = ExternalLoginFactory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }
    }
}
