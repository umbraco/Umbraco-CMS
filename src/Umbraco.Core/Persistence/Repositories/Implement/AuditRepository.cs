using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class AuditRepository : NPocoRepositoryBase<int, IAuditItem>, IAuditRepository
    {
        public AuditRepository(IScopeAccessor scopeAccessor, ILogger logger)
            : base(scopeAccessor, AppCaches.NoCache, logger)
        { }

        protected override void PersistNewItem(IAuditItem entity)
        {
            Database.Insert(new LogDto
            {
                Comment = entity.Comment,
                Datestamp = DateTime.Now,
                Header = entity.AuditType.ToString(),
                NodeId = entity.Id,
                UserId = entity.UserId,
                EntityType = entity.EntityType,
                Parameters = entity.Parameters
            });
        }

        protected override void PersistUpdatedItem(IAuditItem entity)
        {
            // inserting when updating because we never update a log entry, perhaps this should throw?
            Database.Insert(new LogDto
            {
                Comment = entity.Comment,
                Datestamp = DateTime.Now,
                Header = entity.AuditType.ToString(),
                NodeId = entity.Id,
                UserId = entity.UserId,
                EntityType = entity.EntityType,
                Parameters = entity.Parameters
            });
        }

        protected override IAuditItem PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.First<LogDto>(sql);
            return dto == null
                ? null
                : new AuditItem(dto.NodeId, Enum<AuditType>.Parse(dto.Header), dto.UserId ?? Constants.Security.UnknownUserId, dto.EntityType, dto.Comment, dto.Parameters);
        }

        protected override IEnumerable<IAuditItem> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IAuditItem> PerformGetByQuery(IQuery<IAuditItem> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IAuditItem>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<LogDto>(sql);

            return dtos.Select(x => new AuditItem(x.NodeId, Enum<AuditType>.Parse(x.Header), x.UserId ?? Constants.Security.UnknownUserId, x.EntityType, x.Comment, x.Parameters)).ToList();
        }

        public IEnumerable<IAuditItem> Get(AuditType type, IQuery<IAuditItem> query)
        {
            var sqlClause = GetBaseQuery(false)
                .Where("(logHeader=@0)", type.ToString());

            var translator = new SqlTranslator<IAuditItem>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<LogDto>(sql);

            return dtos.Select(x => new AuditItem(x.NodeId, Enum<AuditType>.Parse(x.Header), x.UserId ?? Constants.Security.UnknownUserId, x.EntityType, x.Comment, x.Parameters)).ToList();
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = SqlContext.Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<LogDto>();

            sql
                .From<LogDto>();

            if (!isCount)
                sql.LeftJoin<UserDto>().On<LogDto, UserDto>((left, right) => left.UserId == right.Id);

            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotImplementedException();
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        public void CleanLogs(int maximumAgeOfLogsInMinutes)
        {
            var oldestPermittedLogEntry = DateTime.Now.Subtract(new TimeSpan(0, maximumAgeOfLogsInMinutes, 0));

            Database.Execute(
                "delete from umbracoLog where datestamp < @oldestPermittedLogEntry and logHeader in ('open','system')",
                new {oldestPermittedLogEntry = oldestPermittedLogEntry});
        }

        /// <summary>
        /// Return the audit items as paged result
        /// </summary>
        /// <param name="query">
        /// The query coming from the service
        /// </param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderDirection"></param>
        /// <param name="auditTypeFilter">
        /// Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query or the custom filter
        /// so we need to do that here
        /// </param>
        /// <param name="customFilter">
        /// A user supplied custom filter
        /// </param>
        /// <returns></returns>
        public IEnumerable<IAuditItem> GetPagedResultsByQuery(IQuery<IAuditItem> query, long pageIndex, int pageSize,
            out long totalRecords, Direction orderDirection,
            AuditType[] auditTypeFilter,
            IQuery<IAuditItem> customFilter)
        {
            if (auditTypeFilter == null) auditTypeFilter = Array.Empty<AuditType>();

            var sql = GetBaseQuery(false);

            var translator = new SqlTranslator<IAuditItem>(sql, query ?? Query<IAuditItem>());
            sql = translator.Translate();

            if (customFilter != null)
                foreach (var filterClause in customFilter.GetWhereClauses())
                    sql.Where(filterClause.Item1, filterClause.Item2);

            if (auditTypeFilter.Length > 0)
                foreach (var type in auditTypeFilter)
                    sql.Where("(logHeader=@0)", type.ToString());

            sql = orderDirection == Direction.Ascending
                ? sql.OrderBy("Datestamp")
                : sql.OrderByDescending("Datestamp");

            // get page
            var page = Database.Page<LogDto>(pageIndex + 1, pageSize, sql);
            totalRecords = page.TotalItems;

            var items = page.Items.Select(
                dto => new AuditItem(dto.NodeId, Enum<AuditType>.ParseOrNull(dto.Header) ?? AuditType.Custom, dto.UserId ?? Constants.Security.UnknownUserId, dto.EntityType, dto.Comment, dto.Parameters)).ToList();

            // map the DateStamp
            for (var i = 0; i < items.Count; i++)
                items[i].CreateDate = page.Items[i].Datestamp;

            return items;
        }
    }
}
