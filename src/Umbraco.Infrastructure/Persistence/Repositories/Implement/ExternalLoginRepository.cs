using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    // TODO: We should update this to support both users and members. It means we would remove referential integrity from users
    // and the user/member key would be a GUID (we also need to add a GUID to users)
    internal class ExternalLoginRepository : EntityRepositoryBase<int, IIdentityUserLogin>, IExternalLoginRepository
    {
        public ExternalLoginRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<ExternalLoginRepository> logger)
            : base(scopeAccessor, cache, logger)
        { }

        public void DeleteUserLogins(int memberId) => Database.Delete<ExternalLoginDto>("WHERE userId=@userId", new { userId = memberId });

        public void Save(int userId, IEnumerable<IExternalLogin> logins)
        {
            var sql = Sql()
                .Select<ExternalLoginDto>()
                .From<ExternalLoginDto>()
                .Where<ExternalLoginDto>(x => x.UserId == userId)
                .ForUpdate();

            // deduplicate the logins
            logins = logins.DistinctBy(x => x.ProviderKey + x.LoginProvider).ToList();

            var toUpdate = new Dictionary<int, IExternalLogin>();
            var toDelete = new List<int>();
            var toInsert = new List<IExternalLogin>(logins);

            var existingLogins = Database.Fetch<ExternalLoginDto>(sql);
            
            foreach (var existing in existingLogins)
            {
                var found = logins.FirstOrDefault(x =>
                    x.LoginProvider.Equals(existing.LoginProvider, StringComparison.InvariantCultureIgnoreCase)
                    && x.ProviderKey.Equals(existing.ProviderKey, StringComparison.InvariantCultureIgnoreCase));

                if (found != null)
                {
                    toUpdate.Add(existing.Id, found);
                    // if it's an update then it's not an insert
                    toInsert.RemoveAll(x => x.ProviderKey == found.ProviderKey && x.LoginProvider == found.LoginProvider);
                }
                else
                {
                    toDelete.Add(existing.Id);
                }
            }

            // do the deletes, updates and inserts
            if (toDelete.Count > 0)
            {
                Database.DeleteMany<ExternalLoginDto>().Where(x => toDelete.Contains(x.Id)).Execute();
            }

            foreach (var u in toUpdate)
            {
                Database.Update(ExternalLoginFactory.BuildDto(userId, u.Value, u.Key));
            }

            Database.InsertBulk(toInsert.Select(i => ExternalLoginFactory.BuildDto(userId, i)));
        }

        protected override IIdentityUserLogin PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { id = id });

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

            var sql = GetBaseQuery(false).OrderByDescending<ExternalLoginDto>(x => x.CreateDate);

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
                yield return ExternalLoginFactory.BuildEntity(dto);
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

        protected override string GetBaseWhereClause() => "umbracoExternalLogin.id = @id";

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM umbracoExternalLogin WHERE id = @id"
                };
            return list;
        }

        protected override void PersistNewItem(IIdentityUserLogin entity)
        {
            entity.AddingEntity();

            var dto = ExternalLoginFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IIdentityUserLogin entity)
        {
            entity.UpdatingEntity();

            var dto = ExternalLoginFactory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        /// <summary>
        /// Query for user tokens
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<IIdentityUserToken> Get(IQuery<IIdentityUserToken> query)
        {
            Sql<ISqlContext> sqlClause = GetBaseTokenQuery(false);

            var translator = new SqlTranslator<IIdentityUserToken>(sqlClause, query);
            Sql<ISqlContext> sql = translator.Translate();

            List<ExternalLoginTokenDto> dtos = Database.Fetch<ExternalLoginTokenDto>(sql);

            foreach (ExternalLoginTokenDto dto in dtos)
            {
                yield return ExternalLoginFactory.BuildEntity(dto);
            }
        }

        /// <summary>
        /// Count for user tokens
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int Count(IQuery<IIdentityUserToken> query)
        {
            Sql<ISqlContext> sql = Sql().SelectCount().From<ExternalLoginDto>();
            return Database.ExecuteScalar<int>(sql);
        }

        public void Save(int userId, IEnumerable<IExternalLoginToken> tokens)
        {
            // get the existing logins (provider + id)
            var existingUserLogins = Database
                .Fetch<ExternalLoginDto>(GetBaseQuery(false).Where<ExternalLoginDto>(x => x.UserId == userId))
                .ToDictionary(x => x.LoginProvider, x => x.Id);

            // deduplicate the tokens
            tokens = tokens.DistinctBy(x => x.LoginProvider + x.Name).ToList();

            var providers = tokens.Select(x => x.LoginProvider).Distinct().ToList();

            Sql<ISqlContext> sql = GetBaseTokenQuery(true)
                .WhereIn<ExternalLoginDto>(x => x.LoginProvider, providers)
                .Where<ExternalLoginDto>(x => x.UserId == userId);

            var toUpdate = new Dictionary<int, (IExternalLoginToken externalLoginToken, int externalLoginId)>();
            var toDelete = new List<int>();
            var toInsert = new List<IExternalLoginToken>(tokens);

            var existingTokens = Database.Fetch<ExternalLoginTokenDto>(sql);
            
            foreach (ExternalLoginTokenDto existing in existingTokens)
            {
                IExternalLoginToken found = tokens.FirstOrDefault(x =>
                        x.LoginProvider.InvariantEquals(existing.ExternalLoginDto.LoginProvider)
                        && x.Name.InvariantEquals(existing.Name));

                if (found != null)
                {
                    toUpdate.Add(existing.Id, (found, existing.ExternalLoginId));
                    // if it's an update then it's not an insert
                    toInsert.RemoveAll(x => x.LoginProvider.InvariantEquals(found.LoginProvider) && x.Name.InvariantEquals(found.Name));
                }
                else
                {
                    toDelete.Add(existing.Id);
                }
            }

            // do the deletes, updates and inserts
            if (toDelete.Count > 0)
            {
                Database.DeleteMany<ExternalLoginTokenDto>().Where(x => toDelete.Contains(x.Id)).Execute();
            }

            foreach (KeyValuePair<int, (IExternalLoginToken externalLoginToken, int externalLoginId)> u in toUpdate)
            {
                Database.Update(ExternalLoginFactory.BuildDto(u.Value.externalLoginId, u.Value.externalLoginToken, u.Key));
            }

            var insertDtos = new List<ExternalLoginTokenDto>();
            foreach(IExternalLoginToken t in toInsert)
            {
                if (!existingUserLogins.TryGetValue(t.LoginProvider, out int externalLoginId))
                {
                    throw new InvalidOperationException($"A token was attempted to be saved for login provider {t.LoginProvider} which is not assigned to this user");
                }
                insertDtos.Add(ExternalLoginFactory.BuildDto(externalLoginId, t));
            } 
            Database.InsertBulk(insertDtos);
        }

        private Sql<ISqlContext> GetBaseTokenQuery(bool forUpdate)
            => forUpdate ? Sql()
                .Select<ExternalLoginTokenDto>(r => r.Select(x => x.ExternalLoginDto))                
                .From<ExternalLoginTokenDto>()
                .Append(" WITH (UPDLOCK)") // ensure these table values are locked for updates, the ForUpdate ext method does not work here
                .InnerJoin<ExternalLoginDto>()
                .On<ExternalLoginTokenDto, ExternalLoginDto>(x => x.ExternalLoginId, x => x.Id)
            : Sql()
                .Select<ExternalLoginTokenDto>()
                .AndSelect<ExternalLoginDto>(x => x.LoginProvider, x => x.UserId)
                .From<ExternalLoginTokenDto>()
                .InnerJoin<ExternalLoginDto>()
                .On<ExternalLoginTokenDto, ExternalLoginDto>(x => x.ExternalLoginId, x => x.Id);
    }
}
