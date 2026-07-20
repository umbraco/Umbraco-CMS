namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Shared authorization logic for user group assignment.
/// </summary>
public static class UserGroupAssignmentAuthorization
{
    /// <summary>
    /// Returns the group aliases that the performing user is not authorized to assign.
    /// </summary>
    /// <param name="performingUserGroupAliases">The group aliases the performing user belongs to.</param>
    /// <param name="requestedGroupAliases">The group aliases being assigned to the target user.</param>
    /// <param name="existingGroupAliases">The group aliases the target user currently belongs to.</param>
    /// <returns>
    /// Group aliases that are being added but the performing user does not belong to.
    /// An empty collection means the assignment is authorized.
    /// </returns>
    /// <remarks>
    /// Non-admin users can remove any groups but can only add groups they themselves belong to.
    /// Callers should check for admin status before calling this method, as admins bypass this check.
    /// </remarks>
    public static IReadOnlyList<string> GetUnauthorizedGroupAssignments(
        IEnumerable<string> performingUserGroupAliases,
        IEnumerable<string> requestedGroupAliases,
        IEnumerable<string> existingGroupAliases)
    {
        var performingGroups = performingUserGroupAliases.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        var existingGroups = existingGroupAliases.ToHashSet(StringComparer.InvariantCultureIgnoreCase);

        return requestedGroupAliases
            .Where(alias => existingGroups.Contains(alias) is false && performingGroups.Contains(alias) is false)
            .ToArray();
    }
}
