using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class AuditRepository : PetaPocoRepositoryBase<int, IAuditItem>, IAuditRepository
    {
        public AuditRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
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
            if (auditTypeFilter == null) auditTypeFilter = new AuditType[0];

            var sql = GetBaseQuery(false);

            if (query == null) query = new Query<IAuditItem>();

            var queryHasWhereClause = query.GetWhereClauses().Any();
            var translatorIds = new SqlTranslator<IAuditItem>(sql, query);
            var translatedQuery = translatorIds.Translate();

            var customFilterWheres = customFilter?.GetWhereClauses().ToArray();
            var hasCustomFilter = customFilterWheres != null && customFilterWheres.Length > 0;
            if (hasCustomFilter)
            {
                var filterSql = new Sql();
                foreach (var filterClause in customFilterWheres)
                {
                    filterSql.Append($"AND ({filterClause.Item1})", filterClause.Item2);
                }

                translatedQuery = GetFilteredSqlForPagedResults(translatedQuery, filterSql, queryHasWhereClause);
            }

            if (auditTypeFilter.Length > 0)
            {
                var filterSql = new Sql();
                foreach (var filterClause in auditTypeFilter)
                {
                    filterSql.Append("AND (logHeader = @logHeader)", new { logHeader = filterClause.ToString() });
                }

                translatedQuery = GetFilteredSqlForPagedResults(translatedQuery, filterSql, queryHasWhereClause || hasCustomFilter);
            }

            if (orderDirection == Direction.Descending)
                translatedQuery.OrderByDescending("Datestamp");
            else
                translatedQuery.OrderBy("Datestamp");

            // Get page of results and total count
            var pagedResult = Database.Page<LogDto>(pageIndex + 1, pageSize, translatedQuery);
            totalRecords = pagedResult.TotalItems;

            var pages = pagedResult.Items.Select(
                dto => new AuditItem(dto.NodeId, dto.Comment, Enum<AuditType>.ParseOrNull(dto.Header) ?? AuditType.Custom, dto.UserId)).ToArray();

            //Mapping the DateStamp
            for (var i = 0; i < pages.Length; i++)
            {
                pages[i].CreateDate = pagedResult.Items[i].Datestamp;
            }

            return pages;
        }

        protected override void PersistUpdatedItem(IAuditItem entity)
        {
            Database.Insert(new LogDto
            {
                Comment = entity.Comment,
                Datestamp = DateTime.Now,
                Header = entity.AuditType.ToString(),
                NodeId = entity.Id,
                UserId = entity.UserId
            });
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql()
                .Select(isCount ? "COUNT(*)" : "umbracoLog.id, umbracoLog.userId, umbracoLog.NodeId, umbracoLog.Datestamp, umbracoLog.logHeader, umbracoLog.logComment, umbracoUser.userName, umbracoUser.avatar as userAvatar")
                .From<LogDto>(SqlSyntax);
            if (isCount == false)
            {
                sql = sql.LeftJoin<UserDto>(SqlSyntax).On<UserDto, LogDto>(SqlSyntax, dto => dto.Id, dto => dto.UserId);
            }
            return sql;
        }

        #region Not Implemented - not needed currently

        protected override void PersistNewItem(IAuditItem entity)
        {
            throw new NotImplementedException();
        }

        protected override IAuditItem PerformGet(int id)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IAuditItem> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IAuditItem> PerformGetByQuery(IQuery<IAuditItem> query)
        {
            throw new NotImplementedException();
        }

        protected override string GetBaseWhereClause()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotImplementedException();
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        private Sql GetFilteredSqlForPagedResults(Sql sql, Sql filterSql, bool hasWhereClause)
        {
            Sql filteredSql;

            // Apply filter
            if (filterSql != null)
            {
                //ensure we don't append a WHERE if there is already one
                var sqlFilter = hasWhereClause
                    ? filterSql.SQL
                    : " WHERE " + filterSql.SQL.TrimStart("AND ");

                //NOTE: this is certainly strange - NPoco handles this much better but we need to re-create the sql
                // instance a couple of times to get the parameter order correct, for some reason the first
                // time the arguments don't show up correctly but the SQL argument parameter names are actually updated
                // accordingly - so we re-create it again. In v8 we don't need to do this and it's already taken care of.

                filteredSql = new Sql(sql.SQL, sql.Arguments);
                var args = filteredSql.Arguments.Concat(filterSql.Arguments).ToArray();
                filteredSql = new Sql(
                    string.Format("{0} {1}", filteredSql.SQL, sqlFilter),
                    args);
                filteredSql = new Sql(filteredSql.SQL, args);
            }
            else
            {
                //copy to var so that the original isn't changed
                filteredSql = new Sql(sql.SQL, sql.Arguments);
            }
            return filteredSql;
        }
    }
}
