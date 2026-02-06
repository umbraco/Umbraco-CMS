using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IUserGroup" /> and <see cref="IReadOnlyUserGroup" />.
/// </summary>
public static class UserGroupExtensions
{
    /// <summary>
    ///     Converts an <see cref="IUserGroup" /> to an <see cref="IReadOnlyUserGroup" />.
    /// </summary>
    /// <param name="group">The user group to convert.</param>
    /// <returns>A read-only representation of the user group.</returns>
    public static IReadOnlyUserGroup ToReadOnlyGroup(this IUserGroup group)
    {
        // this will generally always be the case
        if (group is IReadOnlyUserGroup readonlyGroup)
        {
            return readonlyGroup;
        }

        // otherwise create one
        return new ReadOnlyUserGroup(
            group.Id,
            group.Key,
            group.Name,
            group.Description,
            group.Icon,
            group.StartContentId,
            group.StartMediaId,
            group.Alias,
            group.AllowedLanguages,
            group.AllowedSections,
            group.Permissions,
            group.GranularPermissions,
            group.HasAccessToAllLanguages);
    }

    /// <summary>
    ///     Determines whether the specified user group is a system user group.
    /// </summary>
    /// <param name="group">The user group to check.</param>
    /// <returns><c>true</c> if the user group is a system user group; otherwise, <c>false</c>.</returns>
    public static bool IsSystemUserGroup(this IUserGroup group) =>
        IsSystemUserGroup(group.Key);

    /// <summary>
    ///     Determines whether the specified read-only user group is a system user group.
    /// </summary>
    /// <param name="group">The user group to check.</param>
    /// <returns><c>true</c> if the user group is a system user group; otherwise, <c>false</c>.</returns>
    public static bool IsSystemUserGroup(this IReadOnlyUserGroup group) =>
        IsSystemUserGroup(group.Key);

    private static bool IsSystemUserGroup(this Guid? groupKey) =>
        groupKey == Constants.Security.AdminGroupKey
        || groupKey == Constants.Security.SensitiveDataGroupKey
        || groupKey == Constants.Security.TranslatorGroupKey;
}
