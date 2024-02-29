using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds the services for the Umbraco installer
    /// </summary>
    internal static IUmbracoBuilder AddInstaller(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<InstallHelper>();

        // register the installer steps
        builder.Services.AddTransient<PackageMigrationRunner>();

        // Add post migration notification handlers
        builder.AddNotificationHandler<UmbracoPlanExecutedNotification, ClearCsrfCookieHandler>();
        return builder;
    }
}
