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
        return new ReadOnlyUserGroup(group.Id, group.Name, group.Icon, group.StartContentId, group.StartMediaId, group.Alias, group.AllowedLanguages, group.AllowedSections, group.Permissions, group.HasAccessToAllLanguages);
    }

    public static bool IsSystemUserGroup(this IUserGroup group) =>
        IsSystemUserGroup(group.Alias);

    public static bool IsSystemUserGroup(this IReadOnlyUserGroup group) =>
        IsSystemUserGroup(group.Alias);

    private static bool IsSystemUserGroup(this string? groupAlias) =>
        groupAlias == Constants.Security.AdminGroupAlias
        || groupAlias == Constants.Security.SensitiveDataGroupAlias
        || groupAlias == Constants.Security.TranslatorGroupAlias;
}
