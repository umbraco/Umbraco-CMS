using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Extensions;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Composition;

// TODO: Move out of composer
/// <summary>
/// Composer that registers Entity Framework Core services and configurations for Umbraco.
/// </summary>
public class UmbracoEFCoreComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IEFCoreMigrationExecutor, EfCoreMigrationExecutor>();

        builder.AddNotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification, EFCoreCreateTablesNotificationHandler>();
        builder.AddNotificationAsyncHandler<UnattendedInstallNotification, EFCoreCreateTablesNotificationHandler>();

        // UmbracoDbContext — keep UseOpenIddict() so EF Core migrations still include the OpenIddict schema.
        builder.Services.AddUmbracoDbContext<UmbracoDbContext>((provider, options, connectionString, providerName) =>
        {
            options.UseOpenIddict();
        });
        builder.Services.AddUnique<IScopeAccessor>(sp => sp.GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>());
        builder.Services.AddUnique<IScopeProvider>(sp => sp.GetRequiredService<IEFCoreScopeProvider<UmbracoDbContext>>());

        // OpenIddictDbContext — runtime context for OpenIddict with change tracking enabled.
        builder.Services.AddPooledDbContextFactory<OpenIddictDbContext>((sp, options) =>
        {
            options.UseUmbracoDatabaseProvider(sp);
            options.UseOpenIddict();
        });
        builder.Services.AddTransient(sp =>
            sp.GetRequiredService<IDbContextFactory<OpenIddictDbContext>>().CreateDbContext());

        // Point OpenIddict runtime to the dedicated context.
        builder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options
                    .UseEntityFrameworkCore()
                    .UseDbContext<OpenIddictDbContext>();
            });
    }
}


/// <summary>
/// Notification handler that creates EF Core database tables after schema creation or unattended install.
/// </summary>
public class EFCoreCreateTablesNotificationHandler : INotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification>, INotificationAsyncHandler<UnattendedInstallNotification>
{
    private readonly IEFCoreMigrationExecutor _iefCoreMigrationExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreCreateTablesNotificationHandler"/> class.
    /// </summary>
    /// <param name="iefCoreMigrationExecutor">The EF Core migration executor.</param>
    public EFCoreCreateTablesNotificationHandler(IEFCoreMigrationExecutor iefCoreMigrationExecutor)
    {
        _iefCoreMigrationExecutor = iefCoreMigrationExecutor;
    }

    /// <inheritdoc />
    public async Task HandleAsync(UnattendedInstallNotification notification, CancellationToken cancellationToken)
    {
        await HandleAsync();
    }

    /// <inheritdoc />
    public async Task HandleAsync(DatabaseSchemaAndDataCreatedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.RequiresUpgrade is false)
        {
            await HandleAsync();
        }
    }

    private async Task HandleAsync()
    {
        await _iefCoreMigrationExecutor.ExecuteAllMigrationsAsync();
    }
}
