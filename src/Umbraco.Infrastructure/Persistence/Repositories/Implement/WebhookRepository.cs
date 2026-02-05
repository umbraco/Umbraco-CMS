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

public class WebhookRepository : IWebhookRepository
{
    private readonly IEFCoreScopeAccessor<UmbracoDbContext> _scopeAccessor;

    public WebhookRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor) =>
        _scopeAccessor = scopeAccessor;

    public async Task<PagedModel<IWebhook>> GetAllAsync(int skip, int take)
    {
        IEfCoreScope<UmbracoDbContext>? scope = _scopeAccessor.AmbientScope;
        if (scope is null)
        {
            return new PagedModel<IWebhook>
            {
                Items = Enumerable.Empty<IWebhook>(),
                Total = 0,
            };
        }

        List<WebhookDto> webhookDtos = await scope.ExecuteWithContextAsync<List<WebhookDto>>(async db =>
        {
            return await db.Webhooks
                .AsNoTracking()
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
            Total = webhookDtos.Count,
        };
    }

    public async Task<IWebhook> CreateAsync(IWebhook webhook)
    {
        IEfCoreScope<UmbracoDbContext>? scope = _scopeAccessor.AmbientScope;
        if (scope is null)
        {
            throw new InvalidOperationException("No ambient EF Core scope is available.");
        }

        webhook.AddingEntity();
        WebhookDto webhookDto = WebhookFactory.BuildDto(webhook);

        var id = await scope.ExecuteWithContextAsync(async db =>
        {
            // TODO: Check ID and child dtos gets inserted.
            EntityEntry<WebhookDto> entry = await db.Webhooks.AddAsync(webhookDto);
            await db.SaveChangesAsync();
            return entry.Entity.Id;
        });
        webhook.Id = id;

        return webhook;
    }

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
                .AsNoTracking()
                .Where(x => x.Key == key)
                .Include(x => x.Webhook2ContentTypeKeys)
                .Include(x => x.Webhook2Events)
                .Include(x => x.Webhook2Headers)
                .FirstOrDefaultAsync();
        });

        return webhookDto is null ? null : WebhookFactory.BuildEntity(webhookDto);
    }

    public async Task<PagedModel<IWebhook>> GetByIdsAsync(IEnumerable<Guid> keys)
    {
        IEfCoreScope<UmbracoDbContext>? scope = _scopeAccessor.AmbientScope;
        if (scope is null)
        {
            return new PagedModel<IWebhook>
            {
                Items = Enumerable.Empty<IWebhook>(),
                Total = 0,
            };
        }

        List<WebhookDto> webhookDtos = await scope.ExecuteWithContextAsync<List<WebhookDto>>(async db =>
        {
            return await db.Webhooks
                .AsNoTracking()
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

    public async Task<PagedModel<IWebhook>> GetByAliasAsync(string alias)
    {
        IEfCoreScope<UmbracoDbContext>? scope = _scopeAccessor.AmbientScope;
        if (scope is null)
        {
            return new PagedModel<IWebhook>
            {
                Items = Enumerable.Empty<IWebhook>(),
                Total = 0,
            };
        }

        List<WebhookDto> dtos = await scope.ExecuteWithContextAsync(async db =>
        {
            return await db.Webhooks
                .AsNoTracking()
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

    public async Task DeleteAsync(IWebhook webhook)
    {
        IEfCoreScope<UmbracoDbContext>? scope = _scopeAccessor.AmbientScope;
        if (scope is null)
        {
            throw new InvalidOperationException("No ambient EF Core scope is available.");
        }

        await scope.ExecuteWithContextAsync<Task>(async db =>
        {
            db.Webhooks.Remove(WebhookFactory.BuildDto(webhook));
            await db.SaveChangesAsync();
        });
    }

    public async Task UpdateAsync(IWebhook webhook)
    {
        IEfCoreScope<UmbracoDbContext>? scope = _scopeAccessor.AmbientScope;
        if (scope is null)
        {
            throw new InvalidOperationException("No ambient EF Core scope is available.");
        }

        webhook.UpdatingEntity();
        WebhookDto dto = WebhookFactory.BuildDto(webhook);
        await scope.ExecuteWithContextAsync<Task>(async db =>
        {
            db.Webhooks.Update(dto);
            await db.SaveChangesAsync();
        });

        // TODO: Ensure this still works.
        // Delete and re-insert the many to one references (event & entity keys)
        // DeleteManyToOneReferences(dto.Id);
        // InsertManyToOneReferences(webhook);
    }
}
