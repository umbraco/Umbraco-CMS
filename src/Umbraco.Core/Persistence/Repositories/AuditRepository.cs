using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<IAuditItem> GetPagedResultsByQuery(IQuery<IAuditItem> query, long pageIndex, int pageSize, out long totalRecords, Direction orderDirection, IQuery<IAuditItem> customFilter)
        {
            var customFilterWheres = customFilter != null ? customFilter.GetWhereClauses().ToArray() : null;
            var hasCustomFilter = customFilterWheres != null && customFilterWheres.Length > 0;

            if (hasCustomFilter)
            {
                var filterSql = new Sql();
                foreach (var filterClause in customFilterWheres)
                {
                    filterSql.Append(string.Format("AND ({0})", filterClause.Item1), filterClause.Item2);
                }
            }
            
            var sql = GetBaseQuery(false);

            if (orderDirection == Direction.Descending)
                sql.OrderByDescending("Datestamp");
            else
                sql.OrderBy("Datestamp");

            if (query == null) query = new Query<IAuditItem>();
            var translatorIds = new SqlTranslator<IAuditItem>(sql, query);
            var translatedQuery = translatorIds.Translate();

            // Get page of results and total count
            var pagedResult = Database.Page<ReadOnlyLogDto>(pageIndex + 1, pageSize, translatedQuery);
            totalRecords = pagedResult.TotalItems;

            return pagedResult.Items.Select(
                dto => new AuditItem(dto.Id, dto.Comment, Enum<AuditType>.Parse(dto.Header), dto.UserId, dto.UserName, dto.UserAvatar)).ToArray();
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
        
    }
}