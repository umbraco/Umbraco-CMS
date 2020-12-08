using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Infrastructure.Security;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.Security;

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
                .AddUserStore<MembersUserStore>()
                .AddUserManager<IMembersUserManager, MembersUserManager>();
        }

        private static MembersIdentityBuilder BuildUmbracoMembersIdentity(this IServiceCollection services)
        {
            // Services used by Umbraco members identity
            services.TryAddScoped<IUserValidator<MembersIdentityUser>, UserValidator<MembersIdentityUser>>();
            services.TryAddScoped<IPasswordValidator<MembersIdentityUser>, PasswordValidator<MembersIdentityUser>>();
            services.TryAddScoped<IPasswordHasher<MembersIdentityUser>, PasswordHasher<MembersIdentityUser>>();
            return new MembersIdentityBuilder(services);
        }
    }
}
