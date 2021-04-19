using System;
using System.Linq;
using Ganss.XSS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.BackOffice.Middleware;
using Umbraco.Cms.Web.BackOffice.ModelsBuilder;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.BackOffice.Services;
using Umbraco.Cms.Web.BackOffice.SignalR;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.AspNetCore;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco back office
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds all required components to run the Umbraco back office
        /// </summary>
        public static IUmbracoBuilder AddBackOffice(this IUmbracoBuilder builder) => builder
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
                .AddMvcAndRazor()
                .AddWebServer()
                .AddPreviewSupport()
                .AddHostedServices()
                .AddDistributedCache()
                .AddModelsBuilderDashboard()
                .AddUnattedInstallCreateUser()
                .AddExamine();

        /// <summary>
        /// Adds Umbraco preview support
        /// </summary>
        public static IUmbracoBuilder AddPreviewSupport(this IUmbracoBuilder builder)
        {
            builder.Services.AddSignalR();

            return builder;
        }

        /// <summary>
        /// Gets the back office tree collection builder
        /// </summary>
        public static TreeCollectionBuilder Trees(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<TreeCollectionBuilder>();

        public static IUmbracoBuilder AddBackOfficeCore(this IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<KeepAliveMiddleware>();
            builder.Services.ConfigureOptions<ConfigureGlobalOptionsForKeepAliveMiddlware>();
            builder.Services.AddUnique<ServerVariablesParser>();
            builder.Services.AddUnique<BackOfficeAreaRoutes>();
            builder.Services.AddUnique<PreviewRoutes>();
            builder.AddNotificationAsyncHandler<ContentCacheRefresherNotification, PreviewHubUpdater>();
            builder.Services.AddUnique<BackOfficeServerVariables>();
            builder.Services.AddScoped<BackOfficeSessionIdValidator>();
            builder.Services.AddScoped<BackOfficeSecurityStampValidator>();

            // register back office trees
            // the collection builder only accepts types inheriting from TreeControllerBase
            // and will filter out those that are not attributed with TreeAttribute
            var umbracoApiControllerTypes = builder.TypeLoader.GetUmbracoApiControllers().ToList();
            builder.Trees()
                .AddTreeControllers(umbracoApiControllerTypes.Where(x => typeof(TreeControllerBase).IsAssignableFrom(x)));

            builder.AddWebMappingProfiles();

            builder.Services.AddUnique<IPhysicalFileSystem>(factory =>
            {
                var path = "~/";
                var hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
                return new PhysicalFileSystem(
                    factory.GetRequiredService<IIOHelper>(),
                    hostingEnvironment,
                    factory.GetRequiredService<ILogger<PhysicalFileSystem>>(),
                    hostingEnvironment.MapPathContentRoot(path),
                    hostingEnvironment.ToAbsolute(path)
                );
            });

            builder.Services.AddUnique<IIconService, IconService>();
            builder.Services.AddUnique<IHtmlSanitizer>(_ =>
            {
                var sanitizer = new HtmlSanitizer();
                sanitizer.AllowedAttributes.UnionWith(Constants.SvgSanitizer.Attributes);
                sanitizer.AllowedCssProperties.UnionWith(Constants.SvgSanitizer.Attributes);
                sanitizer.AllowedTags.UnionWith(Constants.SvgSanitizer.Tags);
                return sanitizer;
            });
            builder.Services.AddUnique<UnhandledExceptionLoggerMiddleware>();

            return builder;
        }
    }
}
