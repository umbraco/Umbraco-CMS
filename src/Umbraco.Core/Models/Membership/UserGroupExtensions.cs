using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Extensions
{
    public static class UserGroupExtensions
    {
        public static IReadOnlyUserGroup ToReadOnlyGroup(this IUserGroup group)
        {
            //this will generally always be the case
            var readonlyGroup = group as IReadOnlyUserGroup;
            if (readonlyGroup != null) return readonlyGroup;

            //otherwise create one
            return new ReadOnlyUserGroup(group.Id, group.Name, group.Icon, group.StartContentId, group.StartMediaId, group.Alias, group.AllowedLanguages, group.AllowedSections, group.Permissions);
        }

        public static bool IsSystemUserGroup(this IUserGroup group) =>
            IsSystemUserGroup(group.Alias);

        public static bool IsSystemUserGroup(this IReadOnlyUserGroup group) =>
            IsSystemUserGroup(group.Alias);

        private static bool IsSystemUserGroup(this string? groupAlias)
        {
            return groupAlias == Constants.Security.AdminGroupAlias
                   || groupAlias == Constants.Security.SensitiveDataGroupAlias
                   || groupAlias == Constants.Security.TranslatorGroupAlias;
        }
    }
}
