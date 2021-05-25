using System;
using System.Collections.Generic;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the NPoco implementation of <see cref="IConsentRepository"/>.
    /// </summary>
    internal class ConsentRepository : NPocoRepositoryBase<int, IConsent>, IConsentRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentRepository"/> class.
        /// </summary>
        public ConsentRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        /// <inheritdoc />
        protected override Guid NodeObjectTypeId => throw new NotSupportedException();

        /// <inheritdoc />
        protected override IConsent PerformGet(int id)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<IConsent> PerformGetAll(params int[] ids)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<IConsent> PerformGetByQuery(IQuery<IConsent> query)
        {
            var sqlClause = Sql().Select<ConsentDto>().From<ConsentDto>();
            var translator = new SqlTranslator<IConsent>(sqlClause, query);
            var sql = translator.Translate().OrderByDescending<ConsentDto>(x => x.CreateDate);
            return ConsentFactory.BuildEntities(Database.Fetch<ConsentDto>(sql));
        }

        /// <inheritdoc />
        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override string GetBaseWhereClause()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override void PersistNewItem(IConsent entity)
        {
            entity.AddingEntity();

            var dto = ConsentFactory.BuildDto(entity);
            Database.Insert(dto);
            entity.Id = dto.Id;
            entity.ResetDirtyProperties();
        }

        /// <inheritdoc />
        protected override void PersistUpdatedItem(IConsent entity)
        {
            entity.UpdatingEntity();

            var dto = ConsentFactory.BuildDto(entity);
            Database.Update(dto);
            entity.ResetDirtyProperties();

            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IConsent, int>(entity.Id));
        }

        /// <inheritdoc />
        public void ClearCurrent(string source, string context, string action)
        {
            var sql = Sql()
                .Update<ConsentDto>(u => u.Set(x => x.Current, false))
                .Where<ConsentDto>(x => x.Source == source && x.Context == context && x.Action == action && x.Current);
            Database.Execute(sql);
        }
    }
}
