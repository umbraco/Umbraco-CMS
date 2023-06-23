using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.Composition;

public class UmbracoEFCoreComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IEFCoreDatabaseCreator, EfCoreDatabaseCreator>();

        builder.AddNotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification, EFCoreCreateTablesNotificationHandler>();
        builder.AddNotificationAsyncHandler<UnattendedInstallNotification, EFCoreCreateTablesNotificationHandler>();
        builder.Services.AddOpenIddict()

            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                options
                    .UseEntityFrameworkCore()
                    .UseDbContext<UmbracoDbContext>();
            });
    }
}


public class EFCoreCreateTablesNotificationHandler : INotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification>, INotificationAsyncHandler<UnattendedInstallNotification>
{
    private readonly IEFCoreDatabaseCreator _iefCoreDatabaseCreator;

    public EFCoreCreateTablesNotificationHandler(IEFCoreDatabaseCreator iefCoreDatabaseCreator)
    {
        _iefCoreDatabaseCreator = iefCoreDatabaseCreator;
    }

    public async Task HandleAsync(UnattendedInstallNotification notification, CancellationToken cancellationToken)
    {
        await HandleAsync();
    }

    public async Task HandleAsync(DatabaseSchemaAndDataCreatedNotification notification, CancellationToken cancellationToken)
    {
        await HandleAsync();
    }

    private async Task HandleAsync()
    {
        await _iefCoreDatabaseCreator.ExecuteAllMigrationsAsync();
    }
}
