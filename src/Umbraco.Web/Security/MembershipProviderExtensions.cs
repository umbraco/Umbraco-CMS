using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Membership;
using Umbraco.Web.Security.Providers;
using Constants = Umbraco.Cms.Core.Constants;

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
            if (Current.HostingEnvironment.IsHosted)
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

        /// <summary>
        /// Returns true if the provider specified is a built-in Umbraco membership provider
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static bool IsUmbracoMembershipProvider(this MembershipProvider membershipProvider)
        {
            return (membershipProvider is UmbracoMembershipProviderBase);
        }
    }
}
