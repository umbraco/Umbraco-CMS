using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class AuditRepository : EntityRepositoryBase<int, IAuditItem>, IAuditRepository
{
    public AuditRepository(
        IScopeAccessor scopeAccessor,
        ILogger<AuditRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            AppCaches.NoCache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    public IEnumerable<IAuditItem> Get(AuditType type, IQuery<IAuditItem> query)
    {
        Sql<ISqlContext>? sqlClause = GetBaseQuery(false)
            .Where($"({SqlSyntax.GetQuotedColumnName("logHeader")}=@0)", type.ToString());

        var translator = new SqlTranslator<IAuditItem>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<LogDto>? dtos = Database.Fetch<LogDto>(sql);

        return AuditItemFactory.BuildEntities(dtos);
    }

    public void CleanLogs(int maximumAgeOfLogsInMinutes)
    {
        DateTime oldestPermittedLogEntry = DateTime.UtcNow.Subtract(new TimeSpan(0, maximumAgeOfLogsInMinutes, 0));

        var headers = new[] { "open", "system" };
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Delete<LogDto>()
            .Where<LogDto>(c => c.Datestamp < oldestPermittedLogEntry)
            .WhereIn<LogDto>(c => c.Header, headers);
        Database.Execute(sql);
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
        auditTypeFilter ??= [];

        Sql<ISqlContext> sql = GetBaseQuery(false);

        var translator = new SqlTranslator<IAuditItem>(sql, query);
        sql = translator.Translate();

        if (customFilter != null)
        {
            foreach (Tuple<string, object[]> filterClause in customFilter.GetWhereClauses())
            {
                sql = sql.Where(filterClause.Item1, filterClause.Item2);
            }
        }

        if (auditTypeFilter.Length > 0)
        {
            foreach (AuditType type in auditTypeFilter)
            {
                var typeString = type.ToString();
                sql = sql.Where<LogDto>(c => c.Header == typeString);
            }
        }

        sql = orderDirection == Direction.Ascending
            ? sql.OrderBy<LogDto>(c => c.Datestamp)
            : sql.OrderByDescending<LogDto>(c => c.Datestamp);

        // get page
        Page<LogDto>? page = Database.Page<LogDto>(pageIndex + 1, pageSize, sql);
        totalRecords = page.TotalItems;

        return AuditItemFactory.BuildEntities(page.Items);
    }

    protected override void PersistNewItem(IAuditItem entity) =>
        Database.Insert(AuditItemFactory.BuildDto(entity));

    protected override void PersistUpdatedItem(IAuditItem entity) =>

        // inserting when updating because we never update a log entry, perhaps this should throw?
        Database.Insert(AuditItemFactory.BuildDto(entity));

    protected override IAuditItem? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where(GetBaseWhereClause(), new { id });

        LogDto? dto = Database.First<LogDto>(sql);
        return dto == null
            ? null
            : AuditItemFactory.BuildEntity(dto);
    }

    protected override IEnumerable<IAuditItem> PerformGetAll(params int[]? ids) => throw new NotImplementedException();

    protected override IEnumerable<IAuditItem> PerformGetByQuery(IQuery<IAuditItem> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IAuditItem>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<LogDto>? dtos = Database.Fetch<LogDto>(sql);

        return AuditItemFactory.BuildEntities(dtos);
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

    protected override string GetBaseWhereClause() =>
        $"{QuoteTableName(LogDto.TableName)}.{QuoteColumnName("id")} = @id";

    protected override IEnumerable<string> GetDeleteClauses() => throw new NotImplementedException();
}
