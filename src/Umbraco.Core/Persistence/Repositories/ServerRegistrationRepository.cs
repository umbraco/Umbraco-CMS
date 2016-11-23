using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class ServerRegistrationRepository : NPocoRepositoryBase<int, IServerRegistration>, IServerRegistrationRepository
    {
        private IRepositoryCachePolicy<IServerRegistration, int> _cachePolicy;

        public ServerRegistrationRepository(IDatabaseUnitOfWork work, CacheHelper cacheHelper, ILogger logger, IQueryFactory queryFactory)
            : base(work, CacheHelper.CreateDisabledCacheHelper(), logger, queryFactory)
        { }

        protected override IRepositoryCachePolicy<IServerRegistration, int> CachePolicy => _cachePolicy
            ?? (_cachePolicy = new FullDataSetRepositoryCachePolicy<IServerRegistration, int>(RuntimeCache, GetEntityId, /*expires:*/ false));

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
            return GetAll().Any(x => x.Id == id);
        }

        protected override IServerRegistration PerformGet(int id)
        {
            // use the underlying GetAll which force-caches all registrations
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        protected override IEnumerable<IServerRegistration> PerformGetAll(params int[] ids)
        {
            var factory = new ServerRegistrationFactory();
            return Database.Fetch<ServerRegistrationDto>("WHERE id > 0")
                .Select(x => factory.BuildEntity(x));
        }

        protected override IEnumerable<IServerRegistration> PerformGetByQuery(IQuery<IServerRegistration> query)
        {
            throw new NotSupportedException("This repository does not support this method.");
        }

        protected override Sql<SqlContext> GetBaseQuery(bool isCount)
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
            return "id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM umbracoServer WHERE id = @Id"                               
                };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(IServerRegistration entity)
        {
            ((ServerRegistration)entity).AddingEntity();

            var factory = new ServerRegistrationFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IServerRegistration entity)
        {
            ((ServerRegistration)entity).UpdatingEntity();

            var factory = new ServerRegistrationFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        public void DeactiveStaleServers(TimeSpan staleTimeout)
        {
            var timeoutDate = DateTime.Now.Subtract(staleTimeout);

            Database.Update<ServerRegistrationDto>("SET isActive=0, isMaster=0 WHERE lastNotifiedDate < @timeoutDate", new { /*timeoutDate =*/ timeoutDate });
            ClearCache();
        }
    }
}