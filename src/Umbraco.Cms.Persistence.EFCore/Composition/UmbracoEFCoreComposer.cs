using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Persistence.Models;
using Umbraco.Cms.Persistence.EFCore.DbContexts;
using Umbraco.Cms.Persistence.EFCore.Factories;
using Umbraco.Cms.Persistence.EFCore.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Composition;

public class UmbracoEFCoreComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IEFCoreMigrationExecutor, EfCoreMigrationExecutor>();
        builder.Services.AddSingleton<Infrastructure.Persistence.IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
        builder.Services.AddSingleton<Infrastructure.Persistence.IUmbracoDatabaseFactory, Infrastructure.Persistence.UmbracoDatabaseFactory>(); // Add this after the new one to make sure this is the default

        builder.AddNotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification, EFCoreCreateTablesNotificationHandler>();
        builder.AddNotificationAsyncHandler<UnattendedInstallNotification, EFCoreCreateTablesNotificationHandler>();

        builder.Services.AddUmbracoEFCoreContext<UmbracoDbContext>((options, connectionString, providerName) =>
        {
            // Register the entity sets needed by OpenIddict.
            options.UseOpenIddict<UmbracoOpenIddictApplication, UmbracoOpenIddictAuthorization, UmbracoOpenIddictScope, UmbracoOpenIddictToken, string>();
        });

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
