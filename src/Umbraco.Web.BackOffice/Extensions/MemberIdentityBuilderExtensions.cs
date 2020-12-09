using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Infrastructure.Security;

namespace Umbraco.Web.BackOffice.Extensions
{
    public static class MemberIdentityBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="UserManager{TUser}"/> for the <seealso cref="MembersIdentityUser"/>.
        /// </summary>
        /// <typeparam name="TUserManager">The type of the user manager to add.</typeparam>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public static IdentityBuilder AddUserManager<TInterface, TUserManager>(this IdentityBuilder identityBuilder)
            where TUserManager : UserManager<MembersIdentityUser>, TInterface
        {
            identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TUserManager));
            return identityBuilder;
        }
    }
}
