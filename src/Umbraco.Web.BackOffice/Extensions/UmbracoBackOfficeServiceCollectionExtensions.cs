using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Mapping;
using Umbraco.Net;
using Umbraco.Web.Common.AspNetCore;

namespace Umbraco.Extensions
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static void AddUmbracoBackOfficeIdentity(this IServiceCollection services)
        {
            // UmbracoMapper - hack?
            services.AddSingleton<IdentityMapDefinition>();
            services.AddSingleton(s => new MapDefinitionCollection(new[] {s.GetService<IdentityMapDefinition>()}));
            services.AddSingleton<UmbracoMapper>();

            services.AddScoped<IIpResolver, AspNetIpResolver>();

            services.AddIdentity<BackOfficeIdentityUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;

                    // TODO: Configure password configuration
                    /*options.Password.RequiredLength = passwordConfiguration.RequiredLength;
                    options.Password.RequireNonAlphanumeric = passwordConfiguration.RequireNonLetterOrDigit;
                    options.Password.RequireDigit = passwordConfiguration.RequireDigit;
                    options.Password.RequireLowercase = passwordConfiguration.RequireLowercase;
                    options.Password.RequireUppercase = passwordConfiguration.RequireUppercase;
                    options.Lockout.MaxFailedAccessAttempts = passwordConfiguration.MaxFailedAccessAttemptsBeforeLockout;*/

                    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
                    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
                    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
                    options.ClaimsIdentity.SecurityStampClaimType = Constants.Web.SecurityStampClaimType;

                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(30);
                })
                .AddDefaultTokenProviders()
                .AddUserStore<BackOfficeUserStore>()
                .AddUserManager<BackOfficeUserManager>();

            // .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<BackOfficeIdentityUser, IdentityRole>>() // TODO: extract custom claims principal factory
        }
    }
}
