using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class ServerRegistrationRepository : PetaPocoRepositoryBase<int, IServerRegistration>, IServerRegistrationRepository
    {
        private readonly ICacheProvider _staticCache;

        public ServerRegistrationRepository(IDatabaseUnitOfWork work, ICacheProvider staticCache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, CacheHelper.CreateDisabledCacheHelper(), logger, sqlSyntax)
        {
            _staticCache = staticCache;
        }

        protected override int PerformCount(IQuery<IServerRegistration> query)
        {
            throw new NotSupportedException("This repository does not support this method");
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
            // we do NOT want to populate the cache on-demand, because then it might happen
            // during a ReadCommited transaction, and reading the registrations under ReadCommited
            // is NOT safe because they could be updated in the middle of the read.
            //
            // the cache is populated by ReloadCache which should only be called from methods
            // that ensure proper locking (at least, read-lock in ReadCommited) of the repo.

            var all = _staticCache.GetCacheItem<IEnumerable<IServerRegistration>>(CacheKey, Enumerable.Empty<IServerRegistration>);
            return ids.Length == 0 ? all : all.Where(x => ids.Contains(x.Id));
        }

        protected override IEnumerable<IServerRegistration> PerformGetByQuery(IQuery<IServerRegistration> query)
        {
            throw new NotSupportedException("This repository does not support this method");
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<ServerRegistrationDto>(SqlSyntax);
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
            ReloadCache();
        }

        protected override void PersistUpdatedItem(IServerRegistration entity)
        {
            ((ServerRegistration)entity).UpdatingEntity();

            var factory = new ServerRegistrationFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
            ReloadCache();
        }

        public override void PersistDeletedItem(IEntity entity)
        {
            base.PersistDeletedItem(entity);
            ReloadCache();
        }

        private static readonly string CacheKey = GetCacheTypeKey<IServerRegistration>() + "all";

        public void ReloadCache()
        {
            var factory = new ServerRegistrationFactory();
            var all = Database.Fetch<ServerRegistrationDto>("WHERE id > 0")
                .Select(x => factory.BuildEntity(x))
                .Cast<IServerRegistration>()
                .ToArray();
            _staticCache.ClearCacheItem(CacheKey);
            _staticCache.GetCacheItem(CacheKey, () => all);
        }

        public void DeactiveStaleServers(TimeSpan staleTimeout)
        {
            var timeoutDate = DateTime.Now.Subtract(staleTimeout);

            Database.Update<ServerRegistrationDto>("SET isActive=0, isMaster=0 WHERE lastNotifiedDate < @timeoutDate", new { /*timeoutDate =*/ timeoutDate });
            ReloadCache();
        }
    }
}