using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Net;
using Umbraco.Web.Common.AspNetCore;

namespace Umbraco.Extensions
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static void AddUmbracoBackOfficeIdentity(this IServiceCollection services)
        {
            services.AddDataProtection();

            services.TryAddScoped<IIpResolver, AspNetCoreIpResolver>();

            services
                .AddIdentityCore<BackOfficeIdentityUser>()
                .AddDefaultTokenProviders()
                .AddUserStore<BackOfficeUserStore>()
                .AddUserManager<BackOfficeUserManager>()
                .AddClaimsPrincipalFactory<BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>>();

            services.ConfigureOptions<UmbracoBackOfficeIdentityOptions>();
            services.AddScoped<ILookupNormalizer, NopLookupNormalizer>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<BackOfficeIdentityUser>>();

        }

        /// <summary>
        /// Used to configure <see cref="IdentityOptions"/> for the Umbraco Back office
        /// </summary>
        private class UmbracoBackOfficeIdentityOptions : IConfigureOptions<IdentityOptions>
        {
            private readonly IUserPasswordConfiguration _userPasswordConfiguration;

            public UmbracoBackOfficeIdentityOptions(IUserPasswordConfiguration userPasswordConfiguration)
            {
                _userPasswordConfiguration = userPasswordConfiguration;
            }

            public void Configure(IdentityOptions options)
            {
                options.User.RequireUniqueEmail = true;
                options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
                options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
                options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
                options.ClaimsIdentity.SecurityStampClaimType = Constants.Web.SecurityStampClaimType;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(30);

                options.Password.RequiredLength = _userPasswordConfiguration.RequiredLength;
                options.Password.RequireNonAlphanumeric = _userPasswordConfiguration.RequireNonLetterOrDigit;
                options.Password.RequireDigit = _userPasswordConfiguration.RequireDigit;
                options.Password.RequireLowercase = _userPasswordConfiguration.RequireLowercase;
                options.Password.RequireUppercase = _userPasswordConfiguration.RequireUppercase;
                options.Lockout.MaxFailedAccessAttempts = _userPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout;
            }
        }


    }
}
