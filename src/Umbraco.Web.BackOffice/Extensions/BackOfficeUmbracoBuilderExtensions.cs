using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.DependencyInjection;
using Umbraco.Infrastructure.PublishedCache.Extensions;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.Extensions;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco back office
    /// </summary>
    public static class BackOfficeUmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds all required components to run the Umbraco back office
        /// </summary>
        public static IUmbracoBuilder AddBackOffice(this IUmbracoBuilder builder) => builder
                .AddConfiguration()
                .AddUmbracoCore()
                .AddWebComponents()
                .AddRuntimeMinifier()
                .AddBackOfficeAuthentication()
                .AddBackOfficeIdentity()
                .AddBackOfficeAuthorizationPolicies()
                .AddMiniProfiler()
                .AddMvcAndRazor()
                .AddWebServer()
                .AddPreviewSupport()
                .AddHostedServices()
                .AddHttpClients()
                .AddNuCache();

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
                .AddCookie(Core.Constants.Security.BackOfficeAuthenticationType)
                .AddCookie(Core.Constants.Security.BackOfficeExternalAuthenticationType, o =>
                {
                    o.Cookie.Name = Core.Constants.Security.BackOfficeExternalAuthenticationType;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                })

                // Although we don't natively support this, we add it anyways so that if end-users implement the required logic
                // they don't have to worry about manually adding this scheme or modifying the sign in manager
                .AddCookie(Core.Constants.Security.BackOfficeTwoFactorAuthenticationType, o =>
                {
                    o.Cookie.Name = Core.Constants.Security.BackOfficeTwoFactorAuthenticationType;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                });

            builder.Services.ConfigureOptions<ConfigureBackOfficeCookieOptions>();
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
        public static IUmbracoBuilder AddBackOfficeAuthorizationPolicies(this IUmbracoBuilder builder, string backOfficeAuthenticationScheme = Umbraco.Core.Constants.Security.BackOfficeAuthenticationType)
        {
            builder.Services.AddBackOfficeAuthorizationPolicies(backOfficeAuthenticationScheme);

            // TODO: See other TODOs in things like UmbracoApiControllerBase ... AFAIK all of this is only used for the back office
            // so IMO these controllers and the features auth policies should just be moved to the back office project and then this
            // ext method can be removed.
            builder.Services.AddUmbracoCommonAuthorizationPolicies();

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
    }
}
