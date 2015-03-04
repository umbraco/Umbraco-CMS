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
    internal class ServerRegistrationRepository : PetaPocoRepositoryBase<int, ServerRegistration>
    {
        public ServerRegistrationRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        protected override ServerRegistration PerformGet(int id)
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

        protected override IEnumerable<ServerRegistration> PerformGetAll(params int[] ids)
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
                var serverDtos = Database.Fetch<ServerRegistrationDto>("WHERE id > 0");
                foreach (var serverDto in serverDtos)
                {
                    yield return Get(serverDto.Id);
                }
            }
        }

        protected override IEnumerable<ServerRegistration> PerformGetByQuery(IQuery<ServerRegistration> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<ServerRegistration>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<ServerRegistration>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.Id);
            }

        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
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

        protected override void PersistNewItem(ServerRegistration entity)
        {
            entity.AddingEntity();

            var factory = new ServerRegistrationFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(ServerRegistration entity)
        {
            entity.UpdatingEntity();

            var factory = new ServerRegistrationFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        public void DeactiveStaleServers(TimeSpan staleTimeout)
        {
            var timeoutDate = DateTime.UtcNow.Subtract(staleTimeout);

            Database.Update<ServerRegistrationDto>("SET isActive=0 WHERE lastNotifiedDate < @timeoutDate", new {timeoutDate = timeoutDate});
        }

    }
}