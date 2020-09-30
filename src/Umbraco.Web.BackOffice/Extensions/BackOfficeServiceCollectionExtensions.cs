using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Net;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Security;

namespace Umbraco.Extensions
{

    public static class BackOfficeServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required for running the Umbraco back office
        /// </summary>
        /// <param name="services"></param>
        public static void AddUmbracoBackOffice(this IServiceCollection services)
        {
            services.AddAntiforgery();

            // TODO: We had this check in v8 where we don't enable these unless we can run...
            //if (runtimeState.Level != RuntimeLevel.Upgrade && runtimeState.Level != RuntimeLevel.Run) return app;

            services.AddSingleton<IFilterProvider, OverrideAuthorizationFilterProvider>();
            services
                .AddAuthentication(Constants.Security.BackOfficeAuthenticationType)
                .AddCookie(Constants.Security.BackOfficeAuthenticationType);
            // TODO: Need to add more cookie options, see https://github.com/dotnet/aspnetcore/blob/3.0/src/Identity/Core/src/IdentityServiceCollectionExtensions.cs#L45

            services.ConfigureOptions<ConfigureBackOfficeCookieOptions>();
        }

        public static void AddUmbracoPreview(this IServiceCollection services)
        {
            services.AddSignalR();
        }

        /// <summary>
        /// Adds the services required for using Umbraco back office Identity
        /// </summary>
        /// <param name="services"></param>
        public static void AddUmbracoBackOfficeIdentity(this IServiceCollection services)
        {
            services.AddDataProtection();

            services.TryAddScoped<IIpResolver, AspNetCoreIpResolver>();

            services.BuildUmbracoBackOfficeIdentity()
                .AddDefaultTokenProviders()
                .AddUserStore<BackOfficeUserStore>()
                .AddUserManager<IBackOfficeUserManager, BackOfficeUserManager>()
                .AddSignInManager<BackOfficeSignInManager>()
                .AddClaimsPrincipalFactory<BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>>();

            // Configure the options specifically for the UmbracoBackOfficeIdentityOptions instance
            services.ConfigureOptions<ConfigureBackOfficeIdentityOptions>();
            services.ConfigureOptions<ConfigureBackOfficeSecurityStampValidatorOptions>();
        }

        private static IdentityBuilder BuildUmbracoBackOfficeIdentity(this IServiceCollection services)
        {
            // Borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/IdentityServiceCollectionExtensions.cs#L33
            // The reason we need our own is because the Identity system doesn't cater easily for multiple identity systems and particularly being
            // able to configure IdentityOptions to a specific provider since there is no named options. So we have strongly typed options
            // and strongly typed ILookupNormalizer and IdentityErrorDescriber since those are 'global' and we need to be unintrusive.

            // TODO: Could move all of this to BackOfficeComposer?

            // Services used by identity
            services.TryAddScoped<IUserValidator<BackOfficeIdentityUser>, UserValidator<BackOfficeIdentityUser>>();
            services.TryAddScoped<IPasswordValidator<BackOfficeIdentityUser>, PasswordValidator<BackOfficeIdentityUser>>();
            services.TryAddScoped<IPasswordHasher<BackOfficeIdentityUser>>(
                services => new BackOfficePasswordHasher(
                    new LegacyPasswordSecurity(services.GetRequiredService<IOptions<UserPasswordConfigurationSettings>>().Value),
                    services.GetRequiredService<IJsonSerializer>()));
            services.TryAddScoped<IUserConfirmation<BackOfficeIdentityUser>, DefaultUserConfirmation<BackOfficeIdentityUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<BackOfficeIdentityUser>, UserClaimsPrincipalFactory<BackOfficeIdentityUser>>();
            services.TryAddScoped<UserManager<BackOfficeIdentityUser>>();

            // CUSTOM:
            services.TryAddScoped<BackOfficeLookupNormalizer>();
            services.TryAddScoped<BackOfficeIdentityErrorDescriber>();

            return new IdentityBuilder(typeof(BackOfficeIdentityUser), services);
        }
    }
}
