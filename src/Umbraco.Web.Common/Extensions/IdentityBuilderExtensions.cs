using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IdentityBuilder"/>
    /// </summary>
    public static class IdentityBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="UserManager{TUser}"/> for the <seealso cref="MemberIdentityUser"/>.
        /// </summary>
        /// <typeparam name="TInterface">The member manager interface</typeparam>
        /// <typeparam name="TUserManager">The member manager type</typeparam>
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public static IdentityBuilder AddMemberManager<TInterface, TUserManager>(this IdentityBuilder identityBuilder)
            where TUserManager : UserManager<MemberIdentityUser>, TInterface
        {
            identityBuilder.AddUserManager<TUserManager>();
            identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TUserManager));
            identityBuilder.Services.AddScoped(typeof(UserManager<MemberIdentityUser>), factory => factory.GetRequiredService<TInterface>());
            return identityBuilder;
        }

        /// <summary>
        /// Adds a <see cref="SignInManager{TUser}"/> implementation for <seealso cref="MemberIdentityUser"/>
        /// </summary>
        /// <typeparam name="TInterface">The sign in manager interface</typeparam>
        /// <typeparam name="TSignInManager">The sign in manager type</typeparam>
        /// <param name="identityBuilder">The <see cref="IdentityBuilder"/></param>
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public static IdentityBuilder AddSignInManager<TInterface, TSignInManager>(this IdentityBuilder identityBuilder)
            where TSignInManager : SignInManager<MemberIdentityUser>, TInterface
        {
            identityBuilder.AddSignInManager<TSignInManager>();
            identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TSignInManager));
            return identityBuilder;
        }
    }
}
