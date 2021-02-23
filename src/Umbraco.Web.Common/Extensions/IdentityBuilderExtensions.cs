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
        /// Adds a <see cref="UserManager{TUser}"/> for the <seealso cref="MembersIdentityUser"/>.
        /// </summary>
        /// <typeparam name="TInterface">The usermanager interface</typeparam>
        /// <typeparam name="TUserManager">The usermanager type</typeparam>
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public static IdentityBuilder AddMembersManager<TInterface, TUserManager>(this IdentityBuilder identityBuilder)
            where TUserManager : UserManager<MembersIdentityUser>, TInterface
        {
            identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TUserManager));
            return identityBuilder;
        }
    }
}
