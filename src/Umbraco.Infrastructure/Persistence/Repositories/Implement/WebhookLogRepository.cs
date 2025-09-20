using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class WebhookLogRepository : IWebhookLogRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    private IUmbracoDatabase Database
    {
        get
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                throw new NotSupportedException("Need to be executed in a scope");
            }

            return _scopeAccessor.AmbientScope.Database;
        }
    }

    public WebhookLogRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    public async Task CreateAsync(WebhookLog log)
    {
        WebhookLogDto dto = WebhookLogFactory.CreateDto(log);
        var result = await Database.InsertAsync(dto)!;
        var id = Convert.ToInt32(result);
        log.Id = id;
    }

    public async Task<PagedModel<WebhookLog>> GetPagedAsync(int skip, int take)
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<WebhookLogDto>()
            .From<WebhookLogDto>()
            .OrderByDescending<WebhookLogDto>(x => x.Date);

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        Page<WebhookLogDto>? page = await Database.PageAsync<WebhookLogDto>(pageNumber + 1, pageSize, sql)!;

        return new PagedModel<WebhookLog>
        {
            Total = page.TotalItems,
            Items = page.Items.Select(WebhookLogFactory.DtoToEntity),
        };
    }

    public async Task<IEnumerable<WebhookLog>> GetOlderThanDate(DateTime date)
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<WebhookLogDto>()
            .From<WebhookLogDto>()
            .Where<WebhookLogDto>(log => log.Date < date);

        List<WebhookLogDto>? logs = await Database.FetchAsync<WebhookLogDto>(sql);

        return logs.Select(WebhookLogFactory.DtoToEntity);
    }

    public async Task DeleteByIds(int[] ids)
    {
        Sql<ISqlContext> query = Database.SqlContext.Sql()
            .Delete<WebhookLogDto>()
            .WhereIn<WebhookLogDto>(x => x.Id, ids);

        await Database.ExecuteAsync(query);
    }
}
