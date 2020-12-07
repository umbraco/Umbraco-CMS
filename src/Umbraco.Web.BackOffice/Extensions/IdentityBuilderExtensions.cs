using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Security;

namespace Umbraco.Extensions
{
    public static class IdentityBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="UserManager{TUser}"/> implementation for <seealso cref="BackOfficeIdentityUser"/>
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

        /// <summary>
        /// Adds a <see cref="SignInManager{TUser}"/> implementation for <seealso cref="BackOfficeIdentityUser"/>
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TSignInManager"></typeparam>
        /// <param name="identityBuilder"></param>
        /// <returns></returns>
        public static IdentityBuilder AddSignInManager<TInterface, TSignInManager>(this IdentityBuilder identityBuilder) where TSignInManager : SignInManager<BackOfficeIdentityUser>, TInterface
        {
            identityBuilder.AddSignInManager<TSignInManager>();
            identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TSignInManager));
            return identityBuilder;
        }
    }
}
