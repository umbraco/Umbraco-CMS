using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class ServerRegistrationRepository : EntityRepositoryBase<int, IServerRegistration>, IServerRegistrationRepository
    {
        public ServerRegistrationRepository(IScopeAccessor scopeAccessor, ILogger<ServerRegistrationRepository> logger)
            : base(scopeAccessor, AppCaches.NoCache, logger)
        { }

        protected override IRepositoryCachePolicy<IServerRegistration, int> CreateCachePolicy()
        {
            // TODO: what are we doing with cache here?
            // why are we using disabled cache helper up there?
            //
            // 7.6 says:
            // note: this means that the ServerRegistrationRepository does *not* implement scoped cache,
            // and this is because the repository is special and should not participate in scopes
            // (cleanup in v8)
            //
            return new FullDataSetRepositoryCachePolicy<IServerRegistration, int>(AppCaches.RuntimeCache, ScopeAccessor, GetEntityId, /*expires:*/ false);
        }

        public void ClearCache()
        {
            CachePolicy.ClearAll();
        }

        protected override int PerformCount(IQuery<IServerRegistration> query)
        {
            throw new NotSupportedException("This repository does not support this method.");
        }

        protected override bool PerformExists(int id)
        {
            // use the underlying GetAll which force-caches all registrations
            return GetMany()?.Any(x => x.Id == id) ?? false;
        }

        protected override IServerRegistration? PerformGet(int id)
        {
            // use the underlying GetAll which force-caches all registrations
            return GetMany()?.FirstOrDefault(x => x.Id == id);
        }

        protected override IEnumerable<IServerRegistration> PerformGetAll(params int[]? ids)
        {
            return Database.Fetch<ServerRegistrationDto>(GetAllSql())
                .Select(x => ServerRegistrationFactory.BuildEntity(x));
        }
        protected override async Task<IEnumerable<IServerRegistration>> PerformGetAllAsync(params int[]? ids)
        {
            return (await Database.FetchAsync<ServerRegistrationDto>(GetAllSql()))
                .Select(x => ServerRegistrationFactory.BuildEntity(x));
        }

        private static string GetAllSql() => "WHERE id > 0";

        protected override IEnumerable<IServerRegistration> PerformGetByQuery(IQuery<IServerRegistration> query)
        {
            throw new NotSupportedException("This repository does not support this method.");
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<ServerRegistrationDto>();

            sql
               .From<ServerRegistrationDto>();

            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM umbracoServer WHERE id = @id"
                };
            return list;
        }

        protected override void PersistNewItem(IServerRegistration entity)
        {
            entity.AddingEntity();

            var dto = ServerRegistrationFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }
        protected override async Task PersistNewItemAsync(IServerRegistration entity)
        {
            entity.AddingEntity();

            var dto = ServerRegistrationFactory.BuildDto(entity);

            var id = Convert.ToInt32(await Database.InsertAsync(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IServerRegistration entity)
        {
            entity.UpdatingEntity();

            var dto = ServerRegistrationFactory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        protected override async Task PersistUpdatedItemAsync(IServerRegistration entity)
        {
            entity.UpdatingEntity();

            var dto = ServerRegistrationFactory.BuildDto(entity);

            await Database.UpdateAsync(dto);

            entity.ResetDirtyProperties();
        }

        public void DeactiveStaleServers(TimeSpan staleTimeout)
        {
            var timeoutDate = DateTime.Now.Subtract(staleTimeout);

            Database.Update<ServerRegistrationDto>("SET isActive=0, isSchedulingPublisher=0 WHERE lastNotifiedDate < @timeoutDate", new { /*timeoutDate =*/ timeoutDate });
            ClearCache();
        }
    }
}
