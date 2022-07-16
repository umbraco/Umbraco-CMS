using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine.DependencyInjection;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.BackOffice.Install;
using Umbraco.Cms.Web.BackOffice.Middleware;
using Umbraco.Cms.Web.BackOffice.ModelsBuilder;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.BackOffice.Services;
using Umbraco.Cms.Web.BackOffice.SignalR;
using Umbraco.Cms.Web.BackOffice.Trees;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco back office
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all required components to run the Umbraco back office
    /// </summary>
    public static IUmbracoBuilder AddBackOffice(this IUmbracoBuilder builder, Action<IMvcBuilder>? configureMvc = null) => builder
            .AddConfiguration()
            .AddUmbracoCore()
            .AddWebComponents()
            .AddRuntimeMinifier()
            .AddBackOfficeCore()
            .AddBackOfficeAuthentication()
            .AddBackOfficeIdentity()
            .AddMembersIdentity()
            .AddBackOfficeAuthorizationPolicies()
            .AddUmbracoProfiler()
            .AddMvcAndRazor(configureMvc)
            .AddWebServer()
            .AddPreviewSupport()
            .AddHostedServices()
            .AddNuCache()
            .AddDistributedCache()
            .TryAddModelsBuilderDashboard()
            .AddUnattendedInstallInstallCreateUser()
            .AddCoreNotifications()
            .AddLogViewer()
            .AddExamine()
            .AddExamineIndexes()
            .AddControllersWithAmbiguousConstructors()
            .AddSupplemenataryLocalizedTextFileSources();

    public static IUmbracoBuilder AddUnattendedInstallInstallCreateUser(this IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<UnattendedInstallNotification, CreateUnattendedUserNotificationHandler>();
        return builder;
    }

    /// <summary>
    ///     Adds Umbraco preview support
    /// </summary>
    public static IUmbracoBuilder AddPreviewSupport(this IUmbracoBuilder builder)
    {
        builder.Services.AddSignalR();

        return builder;
    }

    /// <summary>
    ///     Gets the back office tree collection builder
    /// </summary>
    public static TreeCollectionBuilder? Trees(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<TreeCollectionBuilder>();

    public static IUmbracoBuilder AddBackOfficeCore(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<KeepAliveMiddleware>();
        builder.Services.ConfigureOptions<ConfigureGlobalOptionsForKeepAliveMiddlware>();
        builder.Services.AddSingleton<ServerVariablesParser>();
        builder.Services.AddSingleton<InstallAreaRoutes>();
        builder.Services.AddSingleton<BackOfficeAreaRoutes>();
        builder.Services.AddSingleton<PreviewRoutes>();
        builder.AddNotificationAsyncHandler<ContentCacheRefresherNotification, PreviewHubUpdater>();
        builder.Services.AddSingleton<BackOfficeServerVariables>();
        builder.Services.AddScoped<BackOfficeSessionIdValidator>();
        builder.Services.AddScoped<BackOfficeSecurityStampValidator>();

        // register back office trees
        // the collection builder only accepts types inheriting from TreeControllerBase
        // and will filter out those that are not attributed with TreeAttribute
        var umbracoApiControllerTypes = builder.TypeLoader.GetUmbracoApiControllers().ToList();
        builder.Trees()?
            .AddTreeControllers(umbracoApiControllerTypes.Where(x => typeof(TreeControllerBase).IsAssignableFrom(x)));

        builder.AddWebMappingProfiles();

        builder.Services.AddUnique<IPhysicalFileSystem>(factory =>
        {
            var path = "~/";
            IHostingEnvironment hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
            return new PhysicalFileSystem(
                factory.GetRequiredService<IIOHelper>(),
                hostingEnvironment,
                factory.GetRequiredService<ILogger<PhysicalFileSystem>>(),
                hostingEnvironment.MapPathContentRoot(path),
                hostingEnvironment.ToAbsolute(path)
            );
        });

        builder.Services.AddUnique<IIconService, IconService>();
        builder.Services.AddUnique<IConflictingRouteService, ConflictingRouteService>();
        builder.Services.AddSingleton<UnhandledExceptionLoggerMiddleware>();

        return builder;
    }

    /// <summary>
    ///     Adds explicit registrations for controllers with ambiguous constructors to prevent downstream issues for
    ///     users who wish to use <see cref="Microsoft.AspNetCore.Mvc.Controllers.ServiceBasedControllerActivator" />
    /// </summary>
    public static IUmbracoBuilder AddControllersWithAmbiguousConstructors(
        this IUmbracoBuilder builder)
    {
        builder.Services.TryAddTransient(sp =>
            ActivatorUtilities.CreateInstance<CurrentUserController>(sp));

        return builder;
    }
}
