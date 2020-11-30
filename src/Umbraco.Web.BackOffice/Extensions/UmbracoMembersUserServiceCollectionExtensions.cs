using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Core.Members;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Infrastructure.Members;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Extensions
{
    public static class UmbracoMembersUserServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required for using Umbraco Members Identity
        /// </summary>
        /// <param name="services"></param>
        public static void AddUmbracoMembersIdentity(this IServiceCollection services)
        {
            services.BuildUmbracoMembersIdentity()
                .AddDefaultTokenProviders()
                .AddUserStore<UmbracoMembersUserStore>()
                .AddUserManager<IUmbracoMembersUserManager, UmbracoMembersUserManager>();
        }

        private static UmbracoMembersIdentityBuilder BuildUmbracoMembersIdentity(this IServiceCollection services)
        {
            // Services used by Umbraco members identity
            services.TryAddScoped<IUserValidator<UmbracoMembersIdentityUser>, UserValidator<UmbracoMembersIdentityUser>>();
            services.TryAddScoped<IPasswordValidator<UmbracoMembersIdentityUser>, PasswordValidator<UmbracoMembersIdentityUser>>();
            services.TryAddScoped<IPasswordHasher<UmbracoMembersIdentityUser>, PasswordHasher<UmbracoMembersIdentityUser>>();
            return new UmbracoMembersIdentityBuilder(services);
        }
    }
}
