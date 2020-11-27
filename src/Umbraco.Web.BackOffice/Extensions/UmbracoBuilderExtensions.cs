using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
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

            // TODO: We need to see if we are 'allowed' to do this, the docs say:
            // "The call to AddIdentity configures the default scheme settings. The AddAuthentication(String) overload sets the DefaultScheme property. The AddAuthentication(Action<AuthenticationOptions>) overload allows configuring authentication options, which can be used to set up default authentication schemes for different purposes. Subsequent calls to AddAuthentication override previously configured AuthenticationOptions properties."
            // So if someone calls services.AddAuthentication() ... in Startup does that overwrite all of this?
            // It also says "When the app requires multiple providers, chain the provider extension methods behind AddAuthentication"
            // Which leads me to believe it all gets overwritten? :/
            // UPDATE: I have tested this breifly in Startup doing Services.AddAuthentication().AddGoogle() ... and the back office auth
            // still seems to work. We'll see how it goes i guess.
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

        public static IUmbracoBuilder AddBackOfficeIdentity(this IUmbracoBuilder builder)
        {
            builder.Services.AddUmbracoBackOfficeIdentity();

            return builder;
        }

        public static IUmbracoBuilder AddPreviewSupport(this IUmbracoBuilder builder)
        {
            builder.Services.AddSignalR();

            return builder;
        }
    }
}
