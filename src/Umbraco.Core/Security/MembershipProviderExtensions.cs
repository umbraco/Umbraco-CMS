﻿using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
{
    public static class MembershipProviderExtensions
    {
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

        internal static MembershipUserCollection FindUsersByName(this MembershipProvider provider, string usernameToMatch)
        {
            int totalRecords = 0;
            return provider.FindUsersByName(usernameToMatch, 0, int.MaxValue, out totalRecords);
        }

        internal static MembershipUserCollection FindUsersByEmail(this MembershipProvider provider, string emailToMatch)
        {
            int totalRecords = 0;
            return provider.FindUsersByEmail(emailToMatch, 0, int.MaxValue, out totalRecords);
        }

        internal static MembershipUser CreateUser(this MembershipProvider provider, string username, string password, string email)
        {
            MembershipCreateStatus status;
            var user = provider.CreateUser(username, password, email, null, null, true, null, out status);
            if (user == null)
                throw new MembershipCreateUserException(status);
            return user;
        }

        /// <summary>
        /// Method to get the Umbraco Members membership provider based on its alias
        /// </summary>
        /// <returns></returns>
        public static MembershipProvider GetMembersMembershipProvider()
        {
            if (Membership.Providers[Constants.Conventions.Member.UmbracoMemberProviderName] == null)
            {
                throw new InvalidOperationException("No membership provider found with name " + Constants.Conventions.Member.UmbracoMemberProviderName);
            }
            return Membership.Providers[Constants.Conventions.Member.UmbracoMemberProviderName];
        }

        /// <summary>
        /// Method to get the Umbraco Users membership provider based on its alias
        /// </summary>
        /// <returns></returns>
        public static MembershipProvider GetUsersMembershipProvider()
        {
            if (Membership.Providers[UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider] == null)
            {
                throw new InvalidOperationException("No membership provider found with name " + UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider);
            }
            return Membership.Providers[UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider];
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

        /// <summary>
        /// Returns true if the provider specified is a built-in Umbraco users provider
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static bool IsUmbracoUsersProvider(this MembershipProvider membershipProvider)
        {
            return (membershipProvider is IUsersMembershipProvider);
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

        //TODO: Add role provider checks too

        public static UmbracoMembershipProviderBase AsUmbracoMembershipProvider(this MembershipProvider membershipProvider)
        {
            return (UmbracoMembershipProviderBase)membershipProvider;
        }
    }
}
