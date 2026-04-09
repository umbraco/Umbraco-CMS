namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Extensions;

/// <summary>
/// Represents a repository responsible for managing webhook entities within the persistence layer of Umbraco CMS.
/// </summary>
public class WebhookRepository : IWebhookRepository
{
    private readonly IEFCoreScopeAccessor<UmbracoDbContext> _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    public WebhookRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor) =>
        _scopeAccessor = scopeAccessor;

    /// <summary>
    /// Asynchronously retrieves a paged list of webhooks.
    /// </summary>
    /// <param name="skip">The number of webhooks to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of webhooks to return.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="PagedModel{IWebhook}"/> with the requested webhooks and the total count.</returns>
    public async Task<PagedModel<IWebhook>> GetAllAsync(int skip, int take)
    {
        IEfCoreScope<UmbracoDbContext> scope = _scopeAccessor.AmbientScope
                                               ?? throw new InvalidOperationException("No ambient EF Core scope is available.");

        List<WebhookDto> webhookDtos = await scope.ExecuteWithContextAsync<List<WebhookDto>>(async db =>
        {
            return await db.Webhooks
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .Include(x => x.Webhook2ContentTypeKeys)
                .Include(x => x.Webhook2Events)
                .Include(x => x.Webhook2Headers)
                .ToListAsync();
        });

        return new PagedModel<IWebhook>
        {
            Items = webhookDtos.Select(WebhookFactory.BuildEntity),
            Total = await scope.ExecuteWithContextAsync(async db => await db.Webhooks.CountAsync()),
        };
    }

    /// <summary>
    /// Asynchronously creates and persists a new webhook entity in the database.
    /// </summary>
    /// <param name="webhook">The <see cref="IWebhook"/> instance to create and persist.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the <see cref="IWebhook"/> instance with its identifier populated after creation.
    /// </returns>
    public async Task<IWebhook> CreateAsync(IWebhook webhook)
    {
        IEfCoreScope<UmbracoDbContext> scope = _scopeAccessor.AmbientScope
                                               ?? throw new InvalidOperationException("No ambient EF Core scope is available.");

        webhook.AddingEntity();
        WebhookDto webhookDto = WebhookFactory.BuildDto(webhook);

        var id = await scope.ExecuteWithContextAsync(async db =>
        {
            EntityEntry<WebhookDto> entry = await db.Webhooks.AddAsync(webhookDto);
            await db.SaveChangesAsync();
            return entry.Entity.Id;
        });
        webhook.Id = id;

        return webhook;
    }

    /// <summary>
    /// Gets a webhook entity asynchronously by its unique key.
    /// </summary>
    /// <param name="key">The unique identifier of the webhook to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the webhook entity if found; otherwise, null.</returns>
    public async Task<IWebhook?> GetAsync(Guid key)
    {
        IEfCoreScope<UmbracoDbContext>? scope = _scopeAccessor.AmbientScope;
        if (scope is null)
        {
            return null;
        }

        WebhookDto? webhookDto = await scope.ExecuteWithContextAsync<WebhookDto?>(async db =>
        {
            return await db.Webhooks
                .Where(x => x.Key == key)
                .Include(x => x.Webhook2ContentTypeKeys)
                .Include(x => x.Webhook2Events)
                .Include(x => x.Webhook2Headers)
                .FirstOrDefaultAsync();
        });

        return webhookDto is null ? null : WebhookFactory.BuildEntity(webhookDto);
    }

    /// <summary>
    /// Asynchronously retrieves a paged collection of webhooks matching the specified unique identifiers.
    /// </summary>
    /// <param name="keys">A collection of unique webhook identifiers to retrieve. If empty or null, an empty result is returned.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a <see cref="PagedModel{IWebhook}"/> with the matching webhooks and the total count.
    /// </returns>
    public async Task<PagedModel<IWebhook>> GetByIdsAsync(IEnumerable<Guid> keys)
    {
        IEfCoreScope<UmbracoDbContext> scope = _scopeAccessor.AmbientScope
                                               ?? throw new InvalidOperationException("No ambient EF Core scope is available.");

        List<WebhookDto> webhookDtos = await scope.ExecuteWithContextAsync<List<WebhookDto>>(async db =>
        {
            return await db.Webhooks
                .Where(x => keys.Contains(x.Key))
                .Include(x => x.Webhook2ContentTypeKeys)
                .Include(x => x.Webhook2Events)
                .Include(x => x.Webhook2Headers)
                .ToListAsync();
        });

        return new PagedModel<IWebhook>
        {
            Items = webhookDtos.Select(WebhookFactory.BuildEntity),
            Total = webhookDtos.Count,
        };
    }

    /// <summary>
    /// Asynchronously retrieves a collection of webhooks associated with the specified event alias.
    /// </summary>
    /// <param name="alias">The event alias to filter webhooks by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a <see cref="PagedModel{IWebhook}"/>,
    /// where <c>Items</c> are the webhooks matching the alias and <c>Total</c> is the total count of such webhooks.
    /// </returns>
    public async Task<PagedModel<IWebhook>> GetByAliasAsync(string alias)
    {
        IEfCoreScope<UmbracoDbContext> scope = _scopeAccessor.AmbientScope
                                                ?? throw new InvalidOperationException("No ambient EF Core scope is available.");

        List<WebhookDto> dtos = await scope.ExecuteWithContextAsync(async db =>
        {
            return await db.Webhooks
                .Include(x => x.Webhook2Events)
                .Where(x => x.Webhook2Events.Any(e => e.Event == alias))
                .Include(x => x.Webhook2ContentTypeKeys)
                .Include(x => x.Webhook2Headers)
                .ToListAsync();
        });

        return new PagedModel<IWebhook>
        {
            Items = dtos.Select(WebhookFactory.BuildEntity),
            Total = dtos.Count,
        };
    }

    /// <summary>
    /// Deletes the specified webhook asynchronously.
    /// </summary>
    /// <param name="webhook">The webhook to delete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public async Task DeleteAsync(IWebhook webhook)
    {
        IEfCoreScope<UmbracoDbContext> scope = _scopeAccessor.AmbientScope
                                                ?? throw new InvalidOperationException("No ambient EF Core scope is available.");

        await scope.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.Webhooks.Where(x => x.Key == webhook.Key).ExecuteDeleteAsync();
        });
    }

    /// <summary>
    /// Asynchronously updates the specified webhook and its related references in the database.
    /// </summary>
    /// <param name="webhook">The webhook entity to update.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns
    public async Task UpdateAsync(IWebhook webhook)
    {
        IEfCoreScope<UmbracoDbContext> scope = _scopeAccessor.AmbientScope
            ?? throw new InvalidOperationException("No ambient EF Core scope is available.");

        webhook.UpdatingEntity();
        WebhookDto dto = WebhookFactory.BuildDto(webhook);
        await scope.ExecuteWithContextAsync<Task>(async db =>
        {
            await DeleteManyToOneReferences(db, dto);

            await db.Webhooks.Where(x => x.Id == dto.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Key, dto.Key)
                .SetProperty(x => x.Name, dto.Name)
                .SetProperty(x => x.Description, dto.Description)
                .SetProperty(x => x.Url, dto.Url)
                .SetProperty(x => x.Enabled, dto.Enabled));

            await AddManyToOneReferences(db, dto);
            await db.SaveChangesAsync();
        });
    }

    private static async Task AddManyToOneReferences(UmbracoDbContext db, WebhookDto dto)
    {
        await db.Set<Webhook2EventsDto>().AddRangeAsync(dto.Webhook2Events);
        await db.Set<Webhook2ContentTypeKeysDto>().AddRangeAsync(dto.Webhook2ContentTypeKeys);
        await db.Set<Webhook2HeadersDto>().AddRangeAsync(dto.Webhook2Headers);
    }

    private static async Task DeleteManyToOneReferences(UmbracoDbContext db, WebhookDto dto)
    {
        await db.Set<Webhook2EventsDto>().Where(x => x.WebhookId == dto.Id).ExecuteDeleteAsync();
        await db.Set<Webhook2ContentTypeKeysDto>().Where(x => x.WebhookId == dto.Id).ExecuteDeleteAsync();
        await db.Set<Webhook2HeadersDto>().Where(x => x.WebhookId == dto.Id).ExecuteDeleteAsync();
    }
}
