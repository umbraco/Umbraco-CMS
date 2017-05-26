using Umbraco.Core.Models.Rdbms;
using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    internal static class UserGroupExtensions
    {
        public static IReadOnlyUserGroup ToReadOnlyGroup(this IUserGroup group)
        {
            return new ReadOnlyUserGroup(group.StartContentId, group.StartMediaId, group.Alias, group.AllowedSections);
        }

        public static IReadOnlyUserGroup ToReadOnlyGroup(this UserGroupDto group)
        {
            return new ReadOnlyUserGroup(group.StartContentId, group.StartMediaId, group.Alias, group.UserGroup2AppDtos.Select(x => x.AppAlias).ToArray());
        }
    }
}