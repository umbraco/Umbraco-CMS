using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Infrastructure.Security;
using Umbraco.Web.Common.Security;

namespace Umbraco.Web.BackOffice.Extensions
{
    public static class MembersUserServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required for using Members Identity
        /// </summary>
        /// <param name="services"></param>
        public static void AddMembersIdentity(this IServiceCollection services)
        {
            services.BuildMembersIdentity()
                .AddDefaultTokenProviders()
                .AddUserStore<MembersUserStore>()
                .AddUserManager<IMembersUserManager, MembersUserManager>();
        }

        private static MembersIdentityBuilder BuildMembersIdentity(this IServiceCollection services)
        {
            // Services used by Umbraco members identity
            services.TryAddScoped<IUserValidator<MembersIdentityUser>, UserValidator<MembersIdentityUser>>();
            services.TryAddScoped<IPasswordValidator<MembersIdentityUser>, PasswordValidator<MembersIdentityUser>>();
            services.TryAddScoped<IPasswordHasher<MembersIdentityUser>, PasswordHasher<MembersIdentityUser>>();
            return new MembersIdentityBuilder(services);
        }
    }
}
