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
                .AddErrorDescriber<BackOfficeIdentityErrorDescriber>();

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
