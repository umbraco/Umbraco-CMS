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
    /// Represents the PetaPoco implementatino of <see cref="IAuditEntryRepository"/>.
    /// </summary>
    internal class AuditEntryRepository : PetaPocoRepositoryBase<int, IAuditEntry>, IAuditEntryRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditEntryRepository"/> class.
        /// </summary>
        public AuditEntryRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        { }

        protected override Guid NodeObjectTypeId => throw new NotSupportedException();

        /// <inheritdoc />
        protected override IAuditEntry PerformGet(int id)
        {
            var sql = new Sql()
                .Select("*")
                .From<AuditEntryDto>(SqlSyntax)
                .Where<AuditEntryDto>(x => x.Id == id, SqlSyntax);

            var dto = Database.FirstOrDefault<AuditEntryDto>(sql);
            return dto == null ? null : AuditEntryFactory.BuildEntity(dto);        }

        /// <inheritdoc />
        protected override IEnumerable<IAuditEntry> PerformGetAll(params int[] ids)
        {
            if (ids.Length == 0)
            {
                var sql = new Sql()
                    .Select("*")
                    .From<AuditEntryDto>(SqlSyntax);

                return Database.Fetch<AuditEntryDto>(sql).Select(AuditEntryFactory.BuildEntity);
            }

            var entries = new List<IAuditEntry>();

            foreach (var group in ids.InGroupsOf(2000))
            {
                var sql = new Sql()
                    .Select("*")
                    .From<AuditEntryDto>(SqlSyntax)
                    .WhereIn<AuditEntryDto>(x => x.Id, group, SqlSyntax);

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
        protected override Sql GetBaseQuery(bool isCount)
        {
            return new Sql().Select(isCount ? "COUNT(*)" : "*").From<AuditEntryDto>(SqlSyntax);
        }

        /// <inheritdoc />
        protected override string GetBaseWhereClause()
        {
            return $"{AuditEntryDto.TableName}.id = @Id";
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotSupportedException("Audit entries cannot be deleted.");
        }

        /// <inheritdoc />
        protected override void PersistNewItem(IAuditEntry entity)
        {
            ((Entity) entity).AddingEntity();

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
            var sql = new Sql()
                .Select("*")
                .From<AuditEntryDto>(SqlSyntax)
                .OrderByDescending<AuditEntryDto>(x => x.EventDateUtc, SqlSyntax);

            var page = Database.Page<AuditEntryDto>(pageIndex + 1, pageCount, sql);
            records = page.TotalItems;
            return page.Items.Select(AuditEntryFactory.BuildEntity);
        }

        /// <inheritdoc />
        public bool IsAvailable()
        {
            var tables = SqlSyntax.GetTablesInSchema(Database).ToArray();
            return tables.InvariantContains(AuditEntryDto.TableName);
        }
    }
}
