using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Mapping;
using Umbraco.Web.BackOffice.Identity;

namespace Umbraco.Extensions
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static void AddUmbracoBackOfficeIdentity(this IServiceCollection services)
        {
            services.AddIdentity<BackOfficeIdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddUserStore<BackOfficeUserStore>();

            // .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<BackOfficeIdentityUser, IdentityRole>>() // TODO: extract custom claims principal factory
            // .AddUserManager<BackOfficeUserManager<BackOfficeIdentityUser>>()
        }
    }
}
