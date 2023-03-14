using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class AuditRepository : EntityRepositoryBase<int, IAuditItem>, IAuditRepository
{
    public AuditRepository(IScopeAccessor scopeAccessor, ILogger<AuditRepository> logger)
        : base(scopeAccessor, AppCaches.NoCache, logger)
    {
    }

    public IEnumerable<IAuditItem> Get(AuditType type, IQuery<IAuditItem> query)
    {
        Sql<ISqlContext>? sqlClause = GetBaseQuery(false)
            .Where("(logHeader=@0)", type.ToString());

        var translator = new SqlTranslator<IAuditItem>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<LogDto>? dtos = Database.Fetch<LogDto>(sql);

        return dtos.Select(x => new AuditItem(x.NodeId, Enum<AuditType>.Parse(x.Header), x.UserId ?? Constants.Security.UnknownUserId, x.EntityType, x.Comment, x.Parameters)).ToList();
    }

    public void CleanLogs(int maximumAgeOfLogsInMinutes)
    {
        DateTime oldestPermittedLogEntry = DateTime.Now.Subtract(new TimeSpan(0, maximumAgeOfLogsInMinutes, 0));

        Database.Execute(
            "delete from umbracoLog where datestamp < @oldestPermittedLogEntry and logHeader in ('open','system')",
            new { oldestPermittedLogEntry });
    }

    /// <summary>
    ///     Return the audit items as paged result
    /// </summary>
    /// <param name="query">
    ///     The query coming from the service
    /// </param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="orderDirection"></param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter
    ///     so we need to do that here
    /// </param>
    /// <param name="customFilter">
    ///     A user supplied custom filter
    /// </param>
    /// <returns></returns>
    public IEnumerable<IAuditItem> GetPagedResultsByQuery(
        IQuery<IAuditItem> query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection,
        AuditType[]? auditTypeFilter,
        IQuery<IAuditItem>? customFilter)
    {
        if (auditTypeFilter == null)
        {
            auditTypeFilter = Array.Empty<AuditType>();
        }

        Sql<ISqlContext> sql = GetBaseQuery(false);

        var translator = new SqlTranslator<IAuditItem>(sql, query);
        sql = translator.Translate();

        if (customFilter != null)
        {
            foreach (Tuple<string, object[]> filterClause in customFilter.GetWhereClauses())
            {
                sql.Where(filterClause.Item1, filterClause.Item2);
            }
        }

        if (auditTypeFilter.Length > 0)
        {
            foreach (AuditType type in auditTypeFilter)
            {
                sql.Where("(logHeader=@0)", type.ToString());
            }
        }

        sql = orderDirection == Direction.Ascending
            ? sql.OrderBy("Datestamp")
            : sql.OrderByDescending("Datestamp");

        // get page
        Page<LogDto>? page = Database.Page<LogDto>(pageIndex + 1, pageSize, sql);
        totalRecords = page.TotalItems;

        var items = page.Items.Select(
            dto => new AuditItem(dto.NodeId, Enum<AuditType>.ParseOrNull(dto.Header) ?? AuditType.Custom, dto.UserId ?? Constants.Security.UnknownUserId, dto.EntityType, dto.Comment, dto.Parameters)).ToList();

        // map the DateStamp
        for (var i = 0; i < items.Count; i++)
        {
            items[i].CreateDate = page.Items[i].Datestamp;
        }

        return items;
    }

    protected override void PersistNewItem(IAuditItem entity) =>
        Database.Insert(new LogDto
        {
            Comment = entity.Comment,
            Datestamp = DateTime.Now,
            Header = entity.AuditType.ToString(),
            NodeId = entity.Id,
            UserId = entity.UserId,
            EntityType = entity.EntityType,
            Parameters = entity.Parameters,
        });

    protected override void PersistUpdatedItem(IAuditItem entity) =>

        // inserting when updating because we never update a log entry, perhaps this should throw?
        Database.Insert(new LogDto
        {
            Comment = entity.Comment,
            Datestamp = DateTime.Now,
            Header = entity.AuditType.ToString(),
            NodeId = entity.Id,
            UserId = entity.UserId,
            EntityType = entity.EntityType,
            Parameters = entity.Parameters,
        });

    protected override IAuditItem? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where(GetBaseWhereClause(), new { Id = id });

        LogDto? dto = Database.First<LogDto>(sql);
        return dto == null
            ? null
            : new AuditItem(dto.NodeId, Enum<AuditType>.Parse(dto.Header), dto.UserId ?? Constants.Security.UnknownUserId, dto.EntityType, dto.Comment, dto.Parameters);
    }

    protected override IEnumerable<IAuditItem> PerformGetAll(params int[]? ids) => throw new NotImplementedException();

    protected override IEnumerable<IAuditItem> PerformGetByQuery(IQuery<IAuditItem> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IAuditItem>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<LogDto>? dtos = Database.Fetch<LogDto>(sql);

        return dtos.Select(x => new AuditItem(x.NodeId, Enum<AuditType>.Parse(x.Header), x.UserId ?? Constants.Security.UnknownUserId, x.EntityType, x.Comment, x.Parameters)).ToList();
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<LogDto>();

        sql
            .From<LogDto>();

        if (!isCount)
        {
            sql.LeftJoin<UserDto>().On<LogDto, UserDto>((left, right) => left.UserId == right.Id);
        }

        return sql;
    }

    protected override string GetBaseWhereClause() => "id = @id";

    protected override IEnumerable<string> GetDeleteClauses() => throw new NotImplementedException();
}
