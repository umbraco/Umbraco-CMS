using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_2_0;

internal class AddUserGroup2NodeTable : MigrationBase
{
    public AddUserGroup2NodeTable(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);
        if (!tables.InvariantContains(UserGroup2NodeDto.TableName))
        {
            Create.Table<UserGroup2NodeDto>().Do();
        }

        // Insert if there exists specific permissions today. Can't do it directly in db in any nice way.
        List<UserGroup2NodePermissionDto>? allData = Database.Fetch<UserGroup2NodePermissionDto>();
        UserGroup2NodeDto[] toInsert = allData
            .Select(x => new UserGroup2NodeDto { NodeId = x.NodeId, UserGroupId = x.UserGroupId }).Distinct(
                new DelegateEqualityComparer<UserGroup2NodeDto>(
                    (x, y) => x?.NodeId == y?.NodeId && x?.UserGroupId == y?.UserGroupId,
                    x => x.NodeId.GetHashCode() + x.UserGroupId.GetHashCode())).ToArray();
        Database.InsertBulk(toInsert);
    }
}
