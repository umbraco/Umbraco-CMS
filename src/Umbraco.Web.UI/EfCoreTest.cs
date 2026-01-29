using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Web.UI;

public class EfCoreTest : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IEFCoreScopeProvider<UmbracoDbContext> _scopeProvider;

    public EfCoreTest(IEFCoreScopeProvider<UmbracoDbContext> scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        using IEfCoreScope<UmbracoDbContext> scope = _scopeProvider.CreateScope();

        var webhooks = await scope.ExecuteWithContextAsync(async db => await db.Set<WebhookDto>()
            .Include(w => w.Webhook2ContentTypeKeys)
            .Include(w => w.Webhook2Events)
            .Include(w => w.Webhook2Headers)
            .ToArrayAsync(cancellationToken));


    }
}


public class EfComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, EfCoreTest>();
}
