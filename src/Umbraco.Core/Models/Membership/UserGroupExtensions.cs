using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Models.Membership
{
    internal static class UserGroupExtensions
    {
        public static IReadOnlyUserGroup ToReadOnlyGroup(this IUserGroup group)
        {
            //this will generally always be the case
            var readonlyGroup = group as IReadOnlyUserGroup;
            if (readonlyGroup != null) return readonlyGroup;

            //otherwise create one
            return new ReadOnlyUserGroup(group.Id, group.Name, group.Icon, group.StartContentId, group.StartMediaId, group.Alias, group.AllowedSections, group.Permissions);
        }

        public static bool IsSystemUserGroup(this IUserGroup group) =>
            IsSystemUserGroup(group.Alias);

        public static bool IsSystemUserGroup(this IReadOnlyUserGroup group) =>
            IsSystemUserGroup(group.Alias);

        public static IReadOnlyUserGroup ToReadOnlyGroup(this UserGroupDto group)
        {
            return new ReadOnlyUserGroup(group.Id, group.Name, group.Icon,
                group.StartContentId, group.StartMediaId, group.Alias,
                group.UserGroup2AppDtos.Select(x => x.AppAlias).ToArray(),
                group.DefaultPermissions == null ? Enumerable.Empty<string>() : group.DefaultPermissions.ToCharArray().Select(x => x.ToString()));
        }

        private static bool IsSystemUserGroup(this string groupAlias)
        {
            return groupAlias == Constants.Security.AdminGroupAlias
                   || groupAlias == Constants.Security.SensitiveDataGroupAlias
                   || groupAlias == Constants.Security.TranslatorGroupAlias;
        }
    }
}
