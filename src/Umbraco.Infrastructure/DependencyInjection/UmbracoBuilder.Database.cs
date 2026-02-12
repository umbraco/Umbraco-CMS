using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Extensions;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    internal static IUmbracoBuilder AddDatabase(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IEFCoreMigrationExecutor, EfCoreMigrationExecutor>();

        builder.AddNotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification, EFCoreCreateTablesNotificationHandler>();
        builder.AddNotificationAsyncHandler<UnattendedInstallNotification, EFCoreCreateTablesNotificationHandler>();

        builder.Services.AddUmbracoDbContext<UmbracoDbContext>((provider, options, connectionString, providerName) =>
        {
            options.UseUmbracoDatabaseProvider(provider);
            options.UseOpenIddict();
        });
        builder.Services.AddUnique<IScopeAccessor>(sp => sp.GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>());
        builder.Services.AddUnique<IScopeProvider>(sp => sp.GetRequiredService<IEFCoreScopeProvider<UmbracoDbContext>>());

        builder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options
                    .UseEntityFrameworkCore()
                    .UseDbContext<UmbracoDbContext>();
            });
        return builder;
    }
}
