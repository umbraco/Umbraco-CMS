using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Services
{
    public static class UserServiceExtensions
    {
        public static EntityPermission GetPermissions(this IUserService userService, IUser user, string path)
        {
            var ids = path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.TryConvertTo<int>())
                .Where(x => x.Success)
                .Select(x => x.Result)
                .ToArray();
            if (ids.Length == 0) throw new InvalidOperationException("The path: " + path + " could not be parsed into an array of integers or the path was empty");

            return userService.GetPermissions(user, ids[ids.Length - 1]).FirstOrDefault();
        }

        /// <summary>
        /// Get explicitly assigned permissions for a group and optional node Ids
        /// </summary>
        /// <param name="service"></param>
        /// <param name="group"></param>
        /// <param name="fallbackToDefaultPermissions">
        ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit permissions set
        /// </param>
        /// <param name="nodeIds">Specifying nothing will return all permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        public static EntityPermissionCollection GetPermissions(this IUserService service, IUserGroup group, bool fallbackToDefaultPermissions, params int[] nodeIds)
        {
            return service.GetPermissions(new[] {group}, fallbackToDefaultPermissions, nodeIds);
        }

        /// <summary>
        /// Gets the permissions for the provided group and path
        /// </summary>
        /// <param name="service"></param>
        /// <param name="group"></param>
        /// <param name="path">Path to check permissions for</param>
        /// <param name="fallbackToDefaultPermissions">
        ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit permissions set
        /// </param>
        public static EntityPermissionSet GetPermissionsForPath(this IUserService service, IUserGroup group, string path, bool fallbackToDefaultPermissions = false)
        {
            return service.GetPermissionsForPath(new[] { group }, path, fallbackToDefaultPermissions);
        }

        /// <summary>
        /// Remove all permissions for this user group for all nodes specified
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="groupId"></param>
        /// <param name="entityIds"></param>
        public static void RemoveUserGroupPermissions(this IUserService userService, int groupId, params int[] entityIds)
        {
            userService.ReplaceUserGroupPermissions(groupId, new char[] {}, entityIds);
        }

        /// <summary>
        /// Remove all permissions for this user group for all nodes
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="groupId"></param>
        public static void RemoveUserGroupPermissions(this IUserService userService, int groupId)
        {
            userService.ReplaceUserGroupPermissions(groupId, new char[] { });
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
        internal static IUser CreateUserMappingForCustomProvider(this IUserService userService, MembershipUser member)
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
                var user = new User(
                    member.UserName,
                    member.Email ?? Guid.NewGuid().ToString("N") + "@example.com", //email cannot be empty
                    member.ProviderUserKey == null ? member.UserName : member.ProviderUserKey.ToString(),
                    Guid.NewGuid().ToString("N")); //pass cannot be empty
                userService.Save(user);
                return user;
            }

            return found;
        }

        /// <summary>
        /// Gets the concrete assigned permissions for the provided user and node
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="user"></param>
        /// <param name="nodeId"></param>
        internal static string[] GetAssignedPermissions(this IUserService userService, IUser user, int nodeId)
        {
            var permissionCollection = userService.GetPermissions(user, nodeId);
            return permissionCollection.SelectMany(c => c.AssignedPermissions).Distinct().ToArray();
        }

        internal static IEnumerable<IProfile> GetProfilesById(this IUserService userService, params int[] ids)
        {
            var fullUsers = userService.GetUsersById(ids);

            return fullUsers.Select(user =>
            {
                var asProfile = user as IProfile;
                return asProfile ?? new UserProfile(user.Id, user.Name);
            });

        }
    }
}
