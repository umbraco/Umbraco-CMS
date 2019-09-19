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
        public void ClearCurrent(string source, string context, string action)
        {
            var sql = new Sql($@"UPDATE {SqlSyntax.GetQuotedTableName(ConsentDto.TableName)}
SET {SqlSyntax.GetQuotedColumnName("current")} = @0
WHERE {SqlSyntax.GetQuotedColumnName("source")} = @1 AND {SqlSyntax.GetQuotedColumnName("context")} = @2 AND {SqlSyntax.GetQuotedColumnName("action")} = @3
AND {SqlSyntax.GetQuotedColumnName("current")} <> @0 ", false, source, context, action);

            Database.Execute(sql);
        }

        /// <inheritdoc />
        protected override IConsent PerformGet(int id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<IConsent> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<IConsent> PerformGetByQuery(IQuery<IConsent> query)
        {
            var sqlClause = new Sql().Select("*").From<ConsentDto>(SqlSyntax);
            var translator = new SqlTranslator<IConsent>(sqlClause, query);
            var sql = translator.Translate().OrderByDescending<ConsentDto>(x => x.CreateDate, SqlSyntax);
            return ConsentFactory.BuildEntities(Database.Fetch<ConsentDto>(sql));
        }

        /// <inheritdoc />
        protected override Sql GetBaseQuery(bool isCount)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override string GetBaseWhereClause()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void PersistNewItem(IConsent entity)
        {
            ((Entity) entity).AddingEntity();

            var dto = ConsentFactory.BuildDto(entity);
            Database.Insert(dto);
            entity.Id = dto.Id;
            entity.ResetDirtyProperties();
        }

        /// <inheritdoc />
        protected override void PersistUpdatedItem(IConsent entity)
        {
            ((Entity) entity).UpdatingEntity();

            var dto = ConsentFactory.BuildDto(entity);
            Database.Update(dto);
            entity.ResetDirtyProperties();

            IsolatedCache.ClearCacheItem(GetCacheIdKey<IConsent>(entity.Id));
        }
    }
}
