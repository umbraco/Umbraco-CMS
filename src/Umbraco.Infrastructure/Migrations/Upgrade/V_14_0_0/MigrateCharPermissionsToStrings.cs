using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

[Obsolete("Remove in Umbraco 18.")]
internal class MigrateCharPermissionsToStrings : MigrationBase
{
    private readonly IIdKeyMap _idKeyMap;

    internal static Dictionary<char, IEnumerable<string>> CharToStringPermissionDictionary { get; } =
        new()
        {
            ['I'] = new []{ActionAssignDomain.ActionLetter},
            ['F'] = new []{ActionBrowse.ActionLetter},
            ['O'] = new []{ActionCopy.ActionLetter},
            ['ï'] = new []{ActionCreateBlueprintFromContent.ActionLetter},
            ['D'] = new []{ActionDelete.ActionLetter},
            ['M'] = new []{ActionMove.ActionLetter},
            ['C'] = new []{ActionNew.ActionLetter},
            ['N'] = new []{ActionNotify.ActionLetter},
            ['P'] = new []{ActionProtect.ActionLetter},
            ['U'] = new []{ActionPublish.ActionLetter},
            ['V'] = new []{ActionRestore.ActionLetter},
            ['R'] = new []{ActionRights.ActionLetter},
            ['K'] = new []{ActionRollback.ActionLetter},
            ['S'] = new []{ActionSort.ActionLetter},
            ['Z'] = new []{ActionUnpublish.ActionLetter},
            ['A'] = new []{ActionUpdate.ActionLetter},
        };

    public MigrateCharPermissionsToStrings(IMigrationContext context, IIdKeyMap idKeyMap)
        : base(context)
    {
        _idKeyMap = idKeyMap;
    }

    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission))
        {
            return;
        }

        List<UserGroupDto>? userGroups = Database.Fetch<UserGroupDto>();

        foreach (UserGroupDto userGroupDto in userGroups)
        {
            if (userGroupDto.DefaultPermissions == null)
            {
                continue;
            }

            var permissions = userGroupDto.DefaultPermissions.SelectMany(oldPermission =>
            {
                IEnumerable<string> newPermissions = ReplacePermissionValue(oldPermission);
                return newPermissions.Select(permission => new UserGroup2PermissionDto()
                {
                    Permission = permission, UserGroupKey = userGroupDto.Key
                });
            }).ToHashSet();

            Database.InsertBulk(permissions);

            userGroupDto.DefaultPermissions = null;

            Database.Update(userGroupDto);
        }

        Create.Table<UserGroup2GranularPermissionDto>().Do();


        List<UserGroup2NodePermissionDto>? userGroup2NodePermissionDtos = Database.Fetch<UserGroup2NodePermissionDto>();

        var userGroupIdToKeys = userGroups.ToDictionary(x => x.Id, x => x.Key);
        IEnumerable<UserGroup2GranularPermissionDto> userGroup2GranularPermissionDtos = userGroup2NodePermissionDtos.SelectMany(userGroup2NodePermissionDto =>
        {
            HashSet<string> permissions = userGroup2NodePermissionDto.Permission?.SelectMany(ReplacePermissionValue).ToHashSet() ?? new HashSet<string>();

            return permissions.Select(permission =>
            {
                var uniqueIdAttempt =
                    _idKeyMap.GetKeyForId(userGroup2NodePermissionDto.NodeId, UmbracoObjectTypes.Document);

                if (uniqueIdAttempt.Success is false)
                {
                    throw new InvalidOperationException("Did not find a key for the document id: " +
                                                        userGroup2NodePermissionDto.NodeId);
                }

                return new UserGroup2GranularPermissionDto()
                {
                    Permission = permission,
                    UserGroupKey = userGroupIdToKeys[userGroup2NodePermissionDto.UserGroupId],
                    UniqueId = uniqueIdAttempt.Result,
                    Context = DocumentGranularPermission.ContextType
                };
            });
        });

        Database.InsertBulk(userGroup2GranularPermissionDtos);

        Delete.Table(Constants.DatabaseSchema.Tables.UserGroup2NodePermission).Do();
        Delete.Table(Constants.DatabaseSchema.Tables.UserGroup2Node).Do();
    }

    private IEnumerable<string> ReplacePermissionValue(char oldPermission) => CharToStringPermissionDictionary.TryGetValue(oldPermission, out IEnumerable<string>? newPermission) ? newPermission : oldPermission.ToString().Yield();
}
