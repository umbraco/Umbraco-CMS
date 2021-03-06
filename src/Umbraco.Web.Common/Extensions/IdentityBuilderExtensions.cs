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
            identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TUserManager));
            return identityBuilder;
        }
    }
}
