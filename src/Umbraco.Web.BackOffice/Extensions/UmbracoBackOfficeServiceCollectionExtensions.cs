using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Net;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Security;

namespace Umbraco.Extensions
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
         public static IServiceCollection AddUmbraco(this IServiceCollection services, IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // TODO: We will need to decide on if we want to use the ServiceBasedControllerActivator to create our controllers
            // or use the default IControllerActivator: DefaultControllerActivator (which doesn't directly use the container to resolve controllers)
            // This will affect whether we need to explicitly register controllers in the container like we do today in v8.
            // What we absolutely must do though is make sure we explicitly opt-in to using one or the other *always* for our controllers instead of
            // relying on a global configuration set by a user since if a custom IControllerActivator is used for our own controllers we may not
            // guarantee it will work. And then... is that even possible?

            // TODO: we will need to simplify this and prob just have a one or 2 main method that devs call which call all other required methods,
            // but for now we'll just be explicit with all of them
            services.AddUmbracoConfiguration(config);
            services.AddUmbracoCore(webHostEnvironment, out var factory);
            services.AddUmbracoWebComponents();
            services.AddUmbracoRuntimeMinifier(config);
            services.AddUmbracoBackOffice();
            services.AddUmbracoBackOfficeIdentity();
            services.AddMiniProfiler(options =>
            {
                options.ShouldProfile = request => false; // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
            });

            //We need to have runtime compilation of views when using umbraco. We could consider having only this when a specific config is set.
            //But as far as I can see, there are still precompiled views, even when this is activated, so maybe it is okay.
            services.AddControllersWithViews().AddRazorRuntimeCompilation();


            // If using Kestrel: https://stackoverflow.com/a/55196057
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            return services;
        }

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
                .AddUserManager<BackOfficeUserManager>()
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
                    new LegacyPasswordSecurity(services.GetRequiredService<IUserPasswordConfiguration>()),
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
