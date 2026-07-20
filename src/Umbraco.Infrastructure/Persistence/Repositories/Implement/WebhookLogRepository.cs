using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Provides methods for accessing and managing webhook log entries in the persistence layer.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookLogRepository"/> class with the specified scope accessor.
    /// </summary>
    /// <param name="scopeAccessor">An accessor that provides the current database scope for repository operations.</param>
    public WebhookLogRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    /// <summary>
    /// Asynchronously creates a new webhook log entry in the database and sets the <c>Id</c> property of the provided <paramref name="log"/> instance to the generated identifier.
    /// </summary>
    /// <param name="log">The webhook log to create. Its <c>Id</c> property will be updated after creation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CreateAsync(WebhookLog log)
    {
        WebhookLogDto dto = WebhookLogFactory.CreateDto(log);
        var result = await Database.InsertAsync(dto)!;
        var id = Convert.ToInt32(result);
        log.Id = id;
    }

    /// <summary>
    /// Asynchronously retrieves a paged collection of <see cref="WebhookLog"/> entries.
    /// </summary>
    /// <param name="skip">The number of entries to bypass before starting to collect the result set. Used for paging.</param>
    /// <param name="take">The maximum number of entries to return in the result set.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="PagedModel{WebhookLog}"/> containing the requested page of webhook log entries.</returns>
    public async Task<PagedModel<WebhookLog>> GetPagedAsync(int skip, int take)
        => await GetPagedAsyncInternal(null, skip, take);

    /// <summary>
    /// Asynchronously retrieves a paged list of <see cref="WebhookLog"/> entries for a specified webhook.
    /// </summary>
    /// <param name="webhookKey">The unique identifier of the webhook to retrieve logs for.</param>
    /// <param name="skip">The number of entries to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of entries to return.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="PagedModel{WebhookLog}"/> containing the webhook logs.</returns>
    public async Task<PagedModel<WebhookLog>> GetPagedAsync(Guid webhookKey, int skip, int take)
        => await GetPagedAsyncInternal(webhookKey, skip, take);

    private async Task<PagedModel<WebhookLog>> GetPagedAsyncInternal(Guid? webhookKey, int skip, int take)
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<WebhookLogDto>()
            .From<WebhookLogDto>()
            .Where<WebhookLogDto>(x => !webhookKey.HasValue || x.WebhookKey == webhookKey)
            .OrderByDescending<WebhookLogDto>(x => x.Date);

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        Page<WebhookLogDto>? page = await Database.PageAsync<WebhookLogDto>(pageNumber + 1, pageSize, sql)!;

        return new PagedModel<WebhookLog>
        {
            Total = page.TotalItems,
            Items = page.Items.Select(WebhookLogFactory.DtoToEntity),
        };
    }

    /// <summary>
    /// Asynchronously retrieves all webhook logs with a date earlier than the specified value.
    /// </summary>
    /// <param name="date">The cutoff date; logs older than this date will be returned.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumerable of webhook logs older than the specified date.</returns>
    public async Task<IEnumerable<WebhookLog>> GetOlderThanDate(DateTime date)
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<WebhookLogDto>()
            .From<WebhookLogDto>()
            .Where<WebhookLogDto>(log => log.Date < date);

        List<WebhookLogDto>? logs = await Database.FetchAsync<WebhookLogDto>(sql);

        return logs.Select(WebhookLogFactory.DtoToEntity);
    }

    /// <summary>
    /// Asynchronously deletes webhook log entries that match the specified IDs.
    /// </summary>
    /// <param name="ids">An array of IDs corresponding to the webhook log entries to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteByIds(int[] ids)
    {
        Sql<ISqlContext> query = Database.SqlContext.Sql()
            .Delete<WebhookLogDto>()
            .WhereIn<WebhookLogDto>(x => x.Id, ids);

        await Database.ExecuteAsync(query);
    }
}
