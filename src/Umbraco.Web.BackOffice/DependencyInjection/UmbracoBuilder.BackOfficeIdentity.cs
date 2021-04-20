using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.AspNetCore;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco back office
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds Identity support for Umbraco back office
        /// </summary>
        public static IUmbracoBuilder AddBackOfficeIdentity(this IUmbracoBuilder builder)
        {
            IServiceCollection services = builder.Services;

            services.AddDataProtection();

            builder.BuildUmbracoBackOfficeIdentity()
                .AddDefaultTokenProviders()
                .AddUserStore<BackOfficeUserStore>()
                .AddUserManager<IBackOfficeUserManager, BackOfficeUserManager>()
                .AddSignInManager<IBackOfficeSignInManager, BackOfficeSignInManager>()
                .AddClaimsPrincipalFactory<BackOfficeClaimsPrincipalFactory>()
                .AddErrorDescriber<BackOfficeErrorDescriber>();

            // Configure the options specifically for the UmbracoBackOfficeIdentityOptions instance
            services.ConfigureOptions<ConfigureBackOfficeIdentityOptions>();
            services.ConfigureOptions<ConfigureBackOfficeSecurityStampValidatorOptions>();

            return builder;
        }

        private static BackOfficeIdentityBuilder BuildUmbracoBackOfficeIdentity(this IUmbracoBuilder builder)
        {
            IServiceCollection services = builder.Services;

            services.TryAddScoped<IIpResolver, AspNetCoreIpResolver>();
            services.TryAddSingleton<IBackOfficeExternalLoginProviders, BackOfficeExternalLoginProviders>();
            services.TryAddSingleton<IBackOfficeTwoFactorOptions, NoopBackOfficeTwoFactorOptions>();

            return new BackOfficeIdentityBuilder(services);
        }

        /// <summary>
        /// Adds support for external login providers in Umbraco
        /// </summary>
        public static IUmbracoBuilder AddBackOfficeExternalLogins(this IUmbracoBuilder umbracoBuilder, Action<BackOfficeExternalLoginsBuilder> builder)
        {
            builder(new BackOfficeExternalLoginsBuilder(umbracoBuilder.Services));
            return umbracoBuilder;
        }

    }
}
