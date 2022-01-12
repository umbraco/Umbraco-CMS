using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
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
    internal class ExternalLoginWithKeyRepository : EntityRepositoryBase<int, IIdentityUserLogin>, IExternalLoginWithKeyRepository
    {
        public ExternalLoginWithKeyRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<ExternalLoginWithKeyRepository> logger)
            : base(scopeAccessor, cache, logger)
        { }

        public void DeleteUserLogins(Guid userOrMemberKey) => Database.Delete<ExternalLoginWithKeyDto>("WHERE userOrMemberKey=@userOrMemberKey", new { userOrMemberKey });

        public void Save(Guid userOrMemberKey, IEnumerable<IExternalLogin> logins)
        {
            var sql = Sql()
                .Select<ExternalLoginWithKeyDto>()
                .From<ExternalLoginWithKeyDto>()
                .Where<ExternalLoginWithKeyDto>(x => x.UserOrMemberKey == userOrMemberKey)
                .ForUpdate();

            // deduplicate the logins
            logins = logins.DistinctBy(x => x.ProviderKey + x.LoginProvider).ToList();

            var toUpdate = new Dictionary<int, IExternalLogin>();
            var toDelete = new List<int>();
            var toInsert = new List<IExternalLogin>(logins);

            var existingLogins = Database.Fetch<ExternalLoginWithKeyDto>(sql);

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
                Database.DeleteMany<ExternalLoginWithKeyDto>().Where(x => toDelete.Contains(x.Id)).Execute();
            }

            foreach (var u in toUpdate)
            {
                Database.Update(ExternalLoginWithKeyFactory.BuildDto(userOrMemberKey, u.Value, u.Key));
            }

            Database.InsertBulk(toInsert.Select(i => ExternalLoginWithKeyFactory.BuildDto(userOrMemberKey, i)));
        }

        protected override IIdentityUserLogin PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { id = id });

            var dto = Database.Fetch<ExternalLoginWithKeyDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
            if (dto == null)
                return null;

            var entity = ExternalLoginWithKeyFactory.BuildEntity(dto);

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

            var sql = GetBaseQuery(false).OrderByDescending<ExternalLoginWithKeyDto>(x => x.CreateDate);

            return ConvertFromDtos(Database.Fetch<ExternalLoginWithKeyDto>(sql))
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

        private IEnumerable<IIdentityUserLogin> ConvertFromDtos(IEnumerable<ExternalLoginWithKeyDto> dtos)
        {
            foreach (var entity in dtos.Select(ExternalLoginWithKeyFactory.BuildEntity))
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

            var dtos = Database.Fetch<ExternalLoginWithKeyDto>(sql);

            foreach (var dto in dtos)
            {
                yield return ExternalLoginWithKeyFactory.BuildEntity(dto);
            }
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();
            if (isCount)
                sql.SelectCount();
            else
                sql.SelectAll();
            sql.From<ExternalLoginWithKeyDto>();
            return sql;
        }

        protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.ExternalLogin}.id = @id";

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

            var dto = ExternalLoginWithKeyFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IIdentityUserLogin entity)
        {
            entity.UpdatingEntity();

            var dto = ExternalLoginWithKeyFactory.BuildDto(entity);

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

            List<ExternalLoginTokenWithKeyDto> dtos = Database.Fetch<ExternalLoginTokenWithKeyDto>(sql);

            foreach (ExternalLoginTokenWithKeyDto dto in dtos)
            {
                yield return ExternalLoginWithKeyFactory.BuildEntity(dto);
            }
        }

        /// <summary>
        /// Count for user tokens
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int Count(IQuery<IIdentityUserToken> query)
        {
            Sql<ISqlContext> sql = Sql().SelectCount().From<ExternalLoginWithKeyDto>();
            return Database.ExecuteScalar<int>(sql);
        }

        public void Save(Guid userOrMemberKey, IEnumerable<IExternalLoginToken> tokens)
        {
            // get the existing logins (provider + id)
            var existingUserLogins = Database
                .Fetch<ExternalLoginWithKeyDto>(GetBaseQuery(false).Where<ExternalLoginWithKeyDto>(x => x.UserOrMemberKey == userOrMemberKey))
                .ToDictionary(x => x.LoginProvider, x => x.Id);

            // deduplicate the tokens
            tokens = tokens.DistinctBy(x => x.LoginProvider + x.Name).ToList();

            var providers = tokens.Select(x => x.LoginProvider).Distinct().ToList();

            Sql<ISqlContext> sql = GetBaseTokenQuery(true)
                .WhereIn<ExternalLoginWithKeyDto>(x => x.LoginProvider, providers)
                .Where<ExternalLoginWithKeyDto>(x => x.UserOrMemberKey == userOrMemberKey);

            var toUpdate = new Dictionary<int, (IExternalLoginToken externalLoginToken, int externalLoginId)>();
            var toDelete = new List<int>();
            var toInsert = new List<IExternalLoginToken>(tokens);

            var existingTokens = Database.Fetch<ExternalLoginTokenWithKeyDto>(sql);

            foreach (ExternalLoginTokenWithKeyDto existing in existingTokens)
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
                Database.DeleteMany<ExternalLoginTokenWithKeyDto>().Where(x => toDelete.Contains(x.Id)).Execute();
            }

            foreach (KeyValuePair<int, (IExternalLoginToken externalLoginToken, int externalLoginId)> u in toUpdate)
            {
                Database.Update(ExternalLoginWithKeyFactory.BuildDto(u.Value.externalLoginId, u.Value.externalLoginToken, u.Key));
            }

            var insertDtos = new List<ExternalLoginTokenWithKeyDto>();
            foreach(IExternalLoginToken t in toInsert)
            {
                if (!existingUserLogins.TryGetValue(t.LoginProvider, out int externalLoginId))
                {
                    throw new InvalidOperationException($"A token was attempted to be saved for login provider {t.LoginProvider} which is not assigned to this user");
                }
                insertDtos.Add(ExternalLoginWithKeyFactory.BuildDto(externalLoginId, t));
            }
            Database.InsertBulk(insertDtos);
        }

        private Sql<ISqlContext> GetBaseTokenQuery(bool forUpdate)
            => forUpdate ? Sql()
                .Select<ExternalLoginTokenWithKeyDto>(r => r.Select(x => x.ExternalLoginDto))
                .From<ExternalLoginTokenWithKeyDto>()
                .Append(" WITH (UPDLOCK)") // ensure these table values are locked for updates, the ForUpdate ext method does not work here
                .InnerJoin<ExternalLoginWithKeyDto>()
                .On<ExternalLoginTokenWithKeyDto, ExternalLoginWithKeyDto>(x => x.ExternalLoginId, x => x.Id)
            : Sql()
                .Select<ExternalLoginTokenWithKeyDto>()
                .AndSelect<ExternalLoginWithKeyDto>(x => x.LoginProvider, x => x.UserOrMemberKey)
                .From<ExternalLoginTokenWithKeyDto>()
                .InnerJoin<ExternalLoginWithKeyDto>()
                .On<ExternalLoginTokenWithKeyDto, ExternalLoginWithKeyDto>(x => x.ExternalLoginId, x => x.Id);
    }
}
