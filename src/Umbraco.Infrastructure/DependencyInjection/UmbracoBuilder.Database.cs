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
        return builder;
    }
}
