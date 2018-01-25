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
    /// <summary>
    /// Represents the PetaPoco implementation of <see cref="IConsentRepository"/>.
    /// </summary>
    internal class ConsentRepository : PetaPocoRepositoryBase<int, IConsent>, IConsentRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentRepository"/> class.
        /// </summary>
        public ConsentRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        { }

        /// <inheritdoc />
        protected override Guid NodeObjectTypeId => throw new NotSupportedException();

        /// <inheritdoc />
        protected override IConsent PerformGet(int id)
        {
            var sql = new Sql()
                .Select("*")
                .From<ConsentDto>(SqlSyntax)
                .Where<ConsentDto>(x => x.Id == id, SqlSyntax);

            var dto = Database.FirstOrDefault<ConsentDto>(sql);
            return dto == null ? null : ConsentFactory.BuildEntity(dto);
        }

        /// <inheritdoc />
        protected override IEnumerable<IConsent> PerformGetAll(params int[] ids)
        {
            if (ids.Length == 0)
            {
                var sql = new Sql()
                    .Select("*")
                    .From<ConsentDto>(SqlSyntax);

                return Database.Fetch<ConsentDto>(sql).Select(ConsentFactory.BuildEntity);
            }

            var consents = new List<IConsent>();

            foreach (var group in ids.InGroupsOf(2000))
            {
                var sql = new Sql()
                    .Select("*")
                    .From<ConsentDto>(SqlSyntax)
                    .WhereIn<ConsentDto>(x => x.Id, group, SqlSyntax);

                consents.AddRange(Database.Fetch<ConsentDto>(sql).Select(ConsentFactory.BuildEntity));
            }

            return consents;
        }

        /// <inheritdoc />
        protected override IEnumerable<IConsent> PerformGetByQuery(IQuery<IConsent> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IConsent>(sqlClause, query);
            var sql = translator.Translate();
            return Database.Fetch<ConsentDto>(sql).Select(ConsentFactory.BuildEntity);
        }

        /// <inheritdoc />
        protected override Sql GetBaseQuery(bool isCount)
        {
            return new Sql().Select(isCount ? "COUNT(*)" : "*").From<ConsentDto>(SqlSyntax);
        }

        /// <inheritdoc />
        protected override string GetBaseWhereClause()
        {
            return $"{ConsentDto.TableName}.id = @Id";
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetDeleteClauses()
        {
            return new[]
            {
                $"DELETE FROM {ConsentDto.TableName} WHERE id = @Id"
            };
        }

        /// <inheritdoc />
        protected override void PersistNewItem(IConsent entity)
        {
            ((Entity) entity).AddingEntity();

            var dto = ConsentFactory.BuildDto(entity);
            Database.Insert(dto); // table has a unique index on source+action
            entity.Id = dto.Id;
            entity.ResetDirtyProperties();
        }

        /// <inheritdoc />
        protected override void PersistUpdatedItem(IConsent entity)
        {
            ((Entity) entity).UpdatingEntity();

            var dto = ConsentFactory.BuildDto(entity);
            Database.Update(dto); // table has a unique index on source+action
            entity.ResetDirtyProperties();

            IsolatedCache.ClearCacheItem(GetCacheIdKey<IConsent>(entity.Id));
        }
    }
}
