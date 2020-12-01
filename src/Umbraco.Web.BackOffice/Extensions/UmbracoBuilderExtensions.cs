using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Umbraco.Core.Builder;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.Builder;

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
                .AddUmbracoMembersIdentity()
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
                .AddAuthentication(Core.Constants.Security.BackOfficeAuthenticationType)
                .AddCookie(Core.Constants.Security.BackOfficeAuthenticationType)
                .AddCookie(Core.Constants.Security.BackOfficeExternalAuthenticationType, o =>
                {
                    o.Cookie.Name = Core.Constants.Security.BackOfficeExternalAuthenticationType;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                });

            // TODO: Need to add more cookie options, see https://github.com/dotnet/aspnetcore/blob/3.0/src/Identity/Core/src/IdentityServiceCollectionExtensions.cs#L45

            builder.Services.ConfigureOptions<ConfigureBackOfficeCookieOptions>();
            return builder;
        }

        public static IUmbracoBuilder AddUmbracoMembersIdentity(this IUmbracoBuilder builder)
        {
            builder.Services.AddUmbracoMembersIdentity();

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
