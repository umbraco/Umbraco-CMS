using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Security.TwoFactorAuthenticationMiddleware
{
    /// <summary>
    /// Subclass the default BackOfficeUserManager and extend it to support 2FA
    /// </summary>
    internal class TwoFactorBackOfficeUserStore : BackOfficeUserStore
    {
        public TwoFactorBackOfficeUserStore(IUserService userService, IExternalLoginService externalLoginService, MembershipProviderBase usersMembershipProvider)
            : base(userService, externalLoginService, usersMembershipProvider)
        { }

        /// <summary>
        /// Override to support setting whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user"/><param name="enabled"/>
        /// <returns/>
        /// <remarks>
        /// This method is NOT designed to persist data! It's just meant to assign it, just like this
        /// </remarks>
        public override Task SetTwoFactorEnabledAsync(BackOfficeIdentityUser user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public override Task<bool> GetTwoFactorEnabledAsync(BackOfficeIdentityUser user)
        {
            var database = ApplicationContext.Current.DatabaseContext.Database;
            var result = database.Fetch<TwoFactorDto>("WHERE [userId] = @userId AND [confirmed] = 1", new { userId = user.Id });
            //if there's records for this user then we need to show the two factor screen
            return Task.FromResult(result.Any());
        }
    }
}