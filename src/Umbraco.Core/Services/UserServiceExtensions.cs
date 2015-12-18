using System;
using System.Web.Security;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Services
{
    internal static class UserServiceExtensions
    {
        /// <summary>
        /// Remove all permissions for this user for all nodes specified
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="userId"></param>
        /// <param name="entityIds"></param>
        public static void RemoveUserPermissions(this IUserService userService, int userId, params int[] entityIds)
        {
            userService.ReplaceUserPermissions(userId, new char[] {}, entityIds);
        }

        /// <summary>
        /// Remove all permissions for this user for all nodes
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="userId"></param>
        public static void RemoveUserPermissions(this IUserService userService, int userId)
        {
            userService.ReplaceUserPermissions(userId, new char[] { });
        }

        /// <summary>
        /// Maps a custom provider's information to an umbraco user account
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="member"></param>
        /// <remarks>
        /// To maintain compatibility we have to check the login name if the provider key lookup fails but otherwise
        /// we'll store the provider user key in the login column.
        /// </remarks>
        public static IUser CreateUserMappingForCustomProvider(this IUserService userService, MembershipUser member)
        {
            if (member == null) throw new ArgumentNullException("member");


            var valToLookup = member.ProviderUserKey == null ? member.UserName : member.ProviderUserKey.ToString();
            var found = userService.GetByUsername(valToLookup);
            if (found == null && member.ProviderUserKey != null)
            {
                //try by username
                found = userService.GetByUsername(member.UserName);
            }

            if (found == null)
            {
                var writer = userService.GetUserTypeByAlias("writer");
                if (writer == null)
                {
                    throw new InvalidOperationException("Could not map the custom user to an Umbraco user, no 'writer' user type could be found");
                }
                var user = new User(
                    member.UserName,
                    member.Email ?? Guid.NewGuid().ToString("N") + "@example.com", //email cannot be empty
                    member.ProviderUserKey == null ? member.UserName : member.ProviderUserKey.ToString(),
                    Guid.NewGuid().ToString("N"), //pass cannot be empty
                    writer);
                user.AddAllowedSection(Constants.Applications.Content);
                userService.Save(user);
                return user;
            }

            return found;
        }
    }
}