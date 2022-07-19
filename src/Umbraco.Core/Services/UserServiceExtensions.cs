using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

public static class UserServiceExtensions
{
    public static EntityPermission? GetPermissions(this IUserService userService, IUser? user, string path)
    {
        var ids = path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x =>
                int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                    ? Attempt<int>.Succeed(value)
                    : Attempt<int>.Fail())
            .Where(x => x.Success)
            .Select(x => x.Result)
            .ToArray();
        if (ids.Length == 0)
        {
            throw new InvalidOperationException("The path: " + path +
                                                " could not be parsed into an array of integers or the path was empty");
        }

        return userService.GetPermissions(user, ids[^1]).FirstOrDefault();
    }

    /// <summary>
    ///     Get explicitly assigned permissions for a group and optional node Ids
    /// </summary>
    /// <param name="service"></param>
    /// <param name="group"></param>
    /// <param name="fallbackToDefaultPermissions">
    ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit
    ///     permissions set
    /// </param>
    /// <param name="nodeIds">Specifying nothing will return all permissions for all nodes</param>
    /// <returns>An enumerable list of <see cref="EntityPermission" /></returns>
    public static EntityPermissionCollection GetPermissions(this IUserService service, IUserGroup? group, bool fallbackToDefaultPermissions, params int[] nodeIds) =>
        service.GetPermissions(new[] { group }, fallbackToDefaultPermissions, nodeIds);

    /// <summary>
    ///     Gets the permissions for the provided group and path
    /// </summary>
    /// <param name="service"></param>
    /// <param name="group"></param>
    /// <param name="path">Path to check permissions for</param>
    /// <param name="fallbackToDefaultPermissions">
    ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit
    ///     permissions set
    /// </param>
    public static EntityPermissionSet GetPermissionsForPath(this IUserService service, IUserGroup group, string path, bool fallbackToDefaultPermissions = false) =>
        service.GetPermissionsForPath(new[] { group }, path, fallbackToDefaultPermissions);

    /// <summary>
    ///     Remove all permissions for this user group for all nodes specified
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="groupId"></param>
    /// <param name="entityIds"></param>
    public static void RemoveUserGroupPermissions(this IUserService userService, int groupId, params int[] entityIds) =>
        userService.ReplaceUserGroupPermissions(groupId, null, entityIds);

    /// <summary>
    ///     Remove all permissions for this user group for all nodes
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="groupId"></param>
    public static void RemoveUserGroupPermissions(this IUserService userService, int groupId) =>
        userService.ReplaceUserGroupPermissions(groupId, null);

    public static IEnumerable<IProfile> GetProfilesById(this IUserService userService, params int[] ids)
    {
        IEnumerable<IUser> fullUsers = userService.GetUsersById(ids);

        return fullUsers.Select(user =>
        {
            var asProfile = user as IProfile;
            return asProfile ?? new UserProfile(user.Id, user.Name);
        });
    }

    public static IUser? GetByKey(this IUserService userService, Guid key)
    {
        var id = BitConverter.ToInt32(key.ToByteArray(), 0);
        return userService.GetUserById(id);
    }
}
