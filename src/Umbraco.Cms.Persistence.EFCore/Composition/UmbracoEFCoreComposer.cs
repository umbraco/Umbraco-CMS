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
        builder.Services.AddSingleton<IEFCoreMigrationExecutor, EfCoreMigrationExecutor>();

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
    private readonly IEFCoreMigrationExecutor _iefCoreMigrationExecutor;

    public EFCoreCreateTablesNotificationHandler(IEFCoreMigrationExecutor iefCoreMigrationExecutor)
    {
        _iefCoreMigrationExecutor = iefCoreMigrationExecutor;
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
        await _iefCoreMigrationExecutor.ExecuteAllMigrationsAsync();
    }
}
