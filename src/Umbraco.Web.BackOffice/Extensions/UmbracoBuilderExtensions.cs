using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.DependencyInjection;
using Umbraco.Web.BackOffice.Extensions;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Extensions
{
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddAllBackOfficeComponents(this IUmbracoBuilder builder)
        {
            return builder
                .AddConfiguration()
                .AddUmbracoCore()
                .AddWebComponents()
                .AddRuntimeMinifier()
                .AddBackOffice()
                .AddBackOfficeIdentity()
                .AddMembersIdentity()
                .AddBackOfficeAuthorizationPolicies()
                .AddMiniProfiler()
                .AddMvcAndRazor()
                .AddWebServer()
                .AddPreviewSupport()
                .AddHostedServices()
                .AddHttpClients();
        }

        public static IUmbracoBuilder AddBackOffice(this IUmbracoBuilder builder)
        {
            builder.Services.AddAntiforgery();
            builder.Services.AddSingleton<IFilterProvider, OverrideAuthorizationFilterProvider>();

            builder.Services
                .AddAuthentication() // This just creates a builder, nothing more
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

        public static IUmbracoBuilder AddMembersIdentity(this IUmbracoBuilder builder)
        {
            builder.Services.AddMembersIdentity();

            return builder;
        }

        public static IUmbracoBuilder AddBackOfficeIdentity(this IUmbracoBuilder builder)
        {
            builder.Services.AddUmbracoBackOfficeIdentity();

            return builder;
        }

        public static IUmbracoBuilder AddBackOfficeAuthorizationPolicies(this IUmbracoBuilder builder, string backOfficeAuthenticationScheme = Umbraco.Core.Constants.Security.BackOfficeAuthenticationType)
        {
            builder.Services.AddBackOfficeAuthorizationPolicies(backOfficeAuthenticationScheme);
            // TODO: See other TODOs in things like UmbracoApiControllerBase ... AFAIK all of this is only used for the back office
            // so IMO these controllers and the features auth policies should just be moved to the back office project and then this
            // ext method can be removed.
            builder.Services.AddUmbracoCommonAuthorizationPolicies();

            return builder;
        }

        public static IUmbracoBuilder AddPreviewSupport(this IUmbracoBuilder builder)
        {
            builder.Services.AddSignalR();

            return builder;
        }
    }
}
