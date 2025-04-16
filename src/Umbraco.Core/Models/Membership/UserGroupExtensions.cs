using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Extensions;

public static class UserGroupExtensions
{
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

    public static bool IsSystemUserGroup(this IUserGroup group) =>
        IsSystemUserGroup(group.Key);

    public static bool IsSystemUserGroup(this IReadOnlyUserGroup group) =>
        IsSystemUserGroup(group.Key);

    private static bool IsSystemUserGroup(this Guid? groupKey) =>
        groupKey == Constants.Security.AdminGroupKey
        || groupKey == Constants.Security.SensitiveDataGroupKey
        || groupKey == Constants.Security.TranslatorGroupKey;
}
