using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.BackOffice;

namespace Umbraco.Extensions
{
    public static class IdentityBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="UserManager{TUser}"/> for the <seealso cref="UserType"/>.
        /// </summary>
        /// <typeparam name="TUserManager">The type of the user manager to add.</typeparam>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public static IdentityBuilder AddUserManager<TInterface, TUserManager>(this IdentityBuilder identityBuilder) where TUserManager : UserManager<BackOfficeIdentityUser>, TInterface
        {
            identityBuilder.AddUserManager<TUserManager>();
            identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TUserManager));
            return identityBuilder;
        }
    }
}
