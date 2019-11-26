using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Membership;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Web.Security
{
    public static class MembershipProviderExtensions
    {

        internal static UmbracoMembershipMember AsConcreteMembershipUser(this IMembershipUser member, string providerName)
        {
            var membershipMember = new UmbracoMembershipMember(member, providerName);
            return membershipMember;
        }

        /// <summary>
        /// Extension method to check if a password can be reset based on a given provider and the current request (logged in user)
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="userService"></param>
        /// <returns></returns>
        /// <remarks>
        /// An Admin can always reset the password
        /// </remarks>
        internal static bool CanResetPassword(this MembershipProvider provider, IUserService userService)
        {
            if (provider == null) throw new ArgumentNullException("provider");

            var canReset = provider.EnablePasswordReset;

            if (userService == null) return canReset;

            //we need to check for the special case in which a user is an admin - in which case they can reset the password even if EnablePasswordReset == false
            if (provider.EnablePasswordReset == false)
            {
                var identity = Thread.CurrentPrincipal.GetUmbracoIdentity();
                if (identity != null)
                {
                    var user = userService.GetUserById(identity.Id.TryConvertTo<int>().Result);
                    if (user == null) throw new InvalidOperationException("No user with username " + identity.Username + " found");
                    var userIsAdmin = user.IsAdmin();
                    if (userIsAdmin)
                    {
                        canReset = true;
                    }
                }
            }
            return canReset;
        }

        /// <summary>
        /// Method to get the Umbraco Members membership provider based on its alias
        /// </summary>
        /// <returns></returns>
        public static MembersMembershipProvider GetMembersMembershipProvider()
        {
            if (Membership.Providers[Constants.Conventions.Member.UmbracoMemberProviderName] == null)
            {
                throw new InvalidOperationException("No membership provider found with name " + Constants.Conventions.Member.UmbracoMemberProviderName);
            }
            return (MembersMembershipProvider)Membership.Providers[Constants.Conventions.Member.UmbracoMemberProviderName];
        }

        /// <summary>
        /// Returns the currently logged in MembershipUser and flags them as being online - use sparingly (i.e. login)
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static MembershipUser GetCurrentUserOnline(this MembershipProvider membershipProvider)
        {
            var username = membershipProvider.GetCurrentUserName();
            return username.IsNullOrWhiteSpace()
                ? null
                : membershipProvider.GetUser(username, true);
        }

        /// <summary>
        /// Returns the currently logged in MembershipUser
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        internal static MembershipUser GetCurrentUser(this MembershipProvider membershipProvider)
        {
            var username = membershipProvider.GetCurrentUserName();
            return username.IsNullOrWhiteSpace()
                ? null
                : membershipProvider.GetUser(username, false);
        }

        /// <summary>
        /// Just returns the current user's login name (just a wrapper).
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        internal static string GetCurrentUserName(this MembershipProvider membershipProvider)
        {
            if (HostingEnvironment.IsHosted)
            {
                HttpContext current = HttpContext.Current;
                if (current != null && current.User != null && current.User.Identity != null)
                    return current.User.Identity.Name;
            }
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            if (currentPrincipal == null || currentPrincipal.Identity == null)
                return string.Empty;
            else
                return currentPrincipal.Identity.Name;
        }       
    }
}
