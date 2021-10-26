using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the NPoco implementation of <see cref="IAuditEntryRepository"/>.
    /// </summary>
    internal class AuditEntryRepository : NPocoRepositoryBase<int, IAuditEntry>, IAuditEntryRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditEntryRepository"/> class.
        /// </summary>
        public AuditEntryRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        /// <inheritdoc />
        protected override Guid NodeObjectTypeId => throw new NotSupportedException();

        /// <inheritdoc />
        protected override IAuditEntry PerformGet(int id)
        {
            var sql = Sql()
                .Select<AuditEntryDto>()
                .From<AuditEntryDto>()
                .Where<AuditEntryDto>(x => x.Id == id);

            var dto = Database.FirstOrDefault<AuditEntryDto>(sql);
            return dto == null ? null : AuditEntryFactory.BuildEntity(dto);
        }

        /// <inheritdoc />
        protected override IEnumerable<IAuditEntry> PerformGetAll(params int[] ids)
        {
            if (ids.Length == 0)
            {
                var sql = Sql()
                    .Select<AuditEntryDto>()
                    .From<AuditEntryDto>();

                return Database.Fetch<AuditEntryDto>(sql).Select(AuditEntryFactory.BuildEntity);
            }

            var entries = new List<IAuditEntry>();

            foreach (var group in ids.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                var sql = Sql()
                    .Select<AuditEntryDto>()
                    .From<AuditEntryDto>()
                    .WhereIn<AuditEntryDto>(x => x.Id, group);

                entries.AddRange(Database.Fetch<AuditEntryDto>(sql).Select(AuditEntryFactory.BuildEntity));
            }

            return entries;
        }

        /// <inheritdoc />
        protected override IEnumerable<IAuditEntry> PerformGetByQuery(IQuery<IAuditEntry> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IAuditEntry>(sqlClause, query);
            var sql = translator.Translate();
            return Database.Fetch<AuditEntryDto>(sql).Select(AuditEntryFactory.BuildEntity);
        }

        /// <inheritdoc />
        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();
            sql = isCount ? sql.SelectCount() : sql.Select<AuditEntryDto>();
            sql = sql.From<AuditEntryDto>();
            return sql;
        }

        /// <inheritdoc />
        protected override string GetBaseWhereClause()
        {
            return $"{Constants.DatabaseSchema.Tables.AuditEntry}.id = @id";
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotSupportedException("Audit entries cannot be deleted.");
        }

        /// <inheritdoc />
        protected override void PersistNewItem(IAuditEntry entity)
        {
            entity.AddingEntity();

            var dto = AuditEntryFactory.BuildDto(entity);
            Database.Insert(dto);
            entity.Id = dto.Id;
            entity.ResetDirtyProperties();
        }

        /// <inheritdoc />
        protected override void PersistUpdatedItem(IAuditEntry entity)
        {
            throw new NotSupportedException("Audit entries cannot be updated.");
        }

        /// <inheritdoc />
        public IEnumerable<IAuditEntry> GetPage(long pageIndex, int pageCount, out long records)
        {
            var sql = Sql()
                .Select<AuditEntryDto>()
                .From<AuditEntryDto>()
                .OrderByDescending<AuditEntryDto>(x => x.EventDateUtc);

            var page = Database.Page<AuditEntryDto>(pageIndex + 1, pageCount, sql);
            records = page.TotalItems;
            return page.Items.Select(AuditEntryFactory.BuildEntity);
        }

        /// <inheritdoc />
        public bool IsAvailable()
        {
            var tables = SqlSyntax.GetTablesInSchema(Database).ToArray();
            return tables.InvariantContains(Constants.DatabaseSchema.Tables.AuditEntry);
        }
    }
}
