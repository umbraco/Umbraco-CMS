using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.BackOffice.Middleware;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.BackOffice.Services;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Infrastructure.DependencyInjection;
using Umbraco.Web.WebAssets;
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco back office
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds all required components to run the Umbraco back office
        /// </summary>
        public static IUmbracoBuilder AddBackOffice(this IUmbracoBuilder builder, IWebHostEnvironment webHostEnvironment) => builder
                .AddConfiguration()
                .AddUmbracoCore()
                .AddWebComponents()
                .AddRuntimeMinifier(webHostEnvironment)
                .AddBackOfficeCore()
                .AddBackOfficeAuthentication()
                .AddBackOfficeIdentity()
                .AddBackOfficeAuthorizationPolicies()
                .AddUmbracoProfiler()
                .AddMvcAndRazor()
                .AddWebServer()
                .AddPreviewSupport()
                .AddHostedServices()
                .AddDistributedCache();

        /// <summary>
        /// Adds Umbraco back office authentication requirements
        /// </summary>
        public static IUmbracoBuilder AddBackOfficeAuthentication(this IUmbracoBuilder builder)
        {
            builder.Services.AddAntiforgery();

            builder.Services

                // This just creates a builder, nothing more
                .AddAuthentication()

                // Add our custom schemes which are cookie handlers
                .AddCookie(Constants.Security.BackOfficeAuthenticationType)
                .AddCookie(Constants.Security.BackOfficeExternalAuthenticationType, o =>
                {
                    o.Cookie.Name = Constants.Security.BackOfficeExternalAuthenticationType;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                })

                // Although we don't natively support this, we add it anyways so that if end-users implement the required logic
                // they don't have to worry about manually adding this scheme or modifying the sign in manager
                .AddCookie(Constants.Security.BackOfficeTwoFactorAuthenticationType, o =>
                {
                    o.Cookie.Name = Constants.Security.BackOfficeTwoFactorAuthenticationType;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                });

            builder.Services.ConfigureOptions<ConfigureBackOfficeCookieOptions>();

            builder.Services.AddUnique<PreviewAuthenticationMiddleware>();
            builder.Services.AddUnique<BackOfficeExternalLoginProviderErrorMiddleware>();
            builder.Services.AddUnique<IBackOfficeAntiforgery, BackOfficeAntiforgery>();

            return builder;
        }

        /// <summary>
        /// Adds Identity support for Umbraco back office
        /// </summary>
        public static IUmbracoBuilder AddBackOfficeIdentity(this IUmbracoBuilder builder)
        {
            builder.Services.AddUmbracoBackOfficeIdentity();

            return builder;
        }

        /// <summary>
        /// Adds Umbraco back office authorization policies
        /// </summary>
        public static IUmbracoBuilder AddBackOfficeAuthorizationPolicies(this IUmbracoBuilder builder, string backOfficeAuthenticationScheme = Constants.Security.BackOfficeAuthenticationType)
        {
            builder.Services.AddBackOfficeAuthorizationPolicies(backOfficeAuthenticationScheme);

            builder.Services.AddSingleton<IAuthorizationHandler, FeatureAuthorizeHandler>();

            builder.Services.AddAuthorization(options
                => options.AddPolicy(AuthorizationPolicies.UmbracoFeatureEnabled, policy
                    => policy.Requirements.Add(new FeatureAuthorizeRequirement())));

            return builder;
        }

        /// <summary>
        /// Adds Umbraco preview support
        /// </summary>
        public static IUmbracoBuilder AddPreviewSupport(this IUmbracoBuilder builder)
        {
            builder.Services.AddSignalR();

            return builder;
        }

        /// <summary>
        /// Adds support for external login providers in Umbraco
        /// </summary>
        public static IUmbracoBuilder AddBackOfficeExternalLogins(this IUmbracoBuilder umbracoBuilder, Action<BackOfficeExternalLoginsBuilder> builder)
        {
            builder(new BackOfficeExternalLoginsBuilder(umbracoBuilder.Services));
            return umbracoBuilder;
        }

        /// <summary>
        /// Gets the back office tree collection builder
        /// </summary>
        public static TreeCollectionBuilder Trees(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<TreeCollectionBuilder>();

        public static IUmbracoBuilder AddBackOfficeCore(this IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<ServerVariablesParser>();
            builder.Services.AddUnique<BackOfficeAreaRoutes>();
            builder.Services.AddUnique<PreviewRoutes>();
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
            builder.Services.AddUnique<UnhandledExceptionLoggerMiddleware>();

            return builder;
        }
    }
}
