using System;
using System.Collections.Generic;
using System.Linq;
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
        public ServerRegistrationRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        protected override IServerRegistration PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var serverDto = Database.First<ServerRegistrationDto>(sql);
            if (serverDto == null)
                return null;

            var factory = new ServerRegistrationFactory();
            var entity = factory.BuildEntity(serverDto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            entity.ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<IServerRegistration> PerformGetAll(params int[] ids)
        {
            var factory = new ServerRegistrationFactory();

            if (ids.Any())
            {
                return Database.Fetch<ServerRegistrationDto>("WHERE id in (@ids)", new { ids = ids })
                    .Select(x => factory.BuildEntity(x));
            }

            return Database.Fetch<ServerRegistrationDto>("WHERE id > 0")
                .Select(x => factory.BuildEntity(x));
        }

        protected override IEnumerable<IServerRegistration> PerformGetByQuery(IQuery<IServerRegistration> query)
        {
            var factory = new ServerRegistrationFactory();
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IServerRegistration>(sqlClause, query);
            var sql = translator.Translate();

            return Database.Fetch<ServerRegistrationDto>(sql).Select(x => factory.BuildEntity(x));
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
            var timeoutDate = DateTime.UtcNow.Subtract(staleTimeout);

            Database.Update<ServerRegistrationDto>("SET isActive=0 WHERE lastNotifiedDate < @timeoutDate", new { timeoutDate = timeoutDate });
        }

    }
}