using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the NPoco implementation of <see cref="IConsentRepository"/>.
    /// </summary>
    internal class ConsentRepository : EntityRepositoryBase<int, IConsent>, IAsyncConsentRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentRepository"/> class.
        /// </summary>
        public ConsentRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<ConsentRepository> logger)
            : base(scopeAccessor, cache, logger)
        { }

        /// <inheritdoc />
        protected override IConsent PerformGet(int id)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override IEnumerable<IConsent> PerformGetAll(params int[]? ids)
        {
            throw new NotSupportedException();
        }
        /// <inheritdoc />
        protected override Task<IEnumerable<IConsent>> PerformGetAllAsync(params int[]? ids)
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
        protected override async Task PersistNewItemAsync(IConsent entity)
        {
            entity.AddingEntity();

            var dto = ConsentFactory.BuildDto(entity);
            await Database.InsertAsync(dto);
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
        protected override async Task PersistUpdatedItemAsync(IConsent entity)
        {
            entity.UpdatingEntity();

            var dto = ConsentFactory.BuildDto(entity);
            await Database.UpdateAsync(dto);
            entity.ResetDirtyProperties();

            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IConsent, int>(entity.Id));
        }

        /// <inheritdoc />
        public void ClearCurrent(string source, string context, string action)
        {
            Sql<ISqlContext> sql = ClearCurrentSql(source, context, action);
            Database.Execute(sql);
        }

        /// <inheritdoc />
        public async Task ClearCurrentAsync(string source, string context, string action)
        {
            Sql<ISqlContext> sql = ClearCurrentSql(source, context, action);
            await Database.ExecuteAsync(sql);
        }

        private Sql<ISqlContext> ClearCurrentSql(string source, string context, string action) => Sql()
                        .Update<ConsentDto>(u => u.Set(x => x.Current, false))
                        .Where<ConsentDto>(x => x.Source == source && x.Context == context && x.Action == action && x.Current);
    }
}
