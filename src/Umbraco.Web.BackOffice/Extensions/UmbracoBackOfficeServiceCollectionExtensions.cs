using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.Mapping;
using Umbraco.Web.BackOffice.Identity;

namespace Umbraco.Extensions
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static void AddUmbracoBackOfficeIdentity(this IServiceCollection services, IFactory factory)
        {
            // UmbracoMapper - hack?
            services.AddSingleton<IdentityMapDefinition>();
            services.AddSingleton(s => new MapDefinitionCollection(new[] {s.GetService<IdentityMapDefinition>()}));
            services.AddSingleton<UmbracoMapper>();
            
            services.AddIdentity<BackOfficeIdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddUserStore<BackOfficeUserStore>();

            // .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<BackOfficeIdentityUser, IdentityRole>>() // TODO: extract custom claims principal factory
            // .AddUserManager<BackOfficeUserManager<BackOfficeIdentityUser>>()
        }
    }
}
