using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

public class AddElements : AsyncMigrationBase
{
    public AddElements(IMigrationContext context)
        : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        EnsureElementTreeLock();
        EnsureElementTables();
        EnsureElementRecycleBin();
        EnsureElementStartNodeColumn();
        return Task.CompletedTask;
    }

    private void EnsureElementTreeLock()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.ElementTree);

        LockDto? cacheVersionLock = Database.Fetch<LockDto>(sql).FirstOrDefault();

        if (cacheVersionLock is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.ElementTree, Name = "ElementTree" });
        }
    }

    private void EnsureElementTables()
    {
        if (!TableExists(Constants.DatabaseSchema.Tables.Element))
        {
            Create.Table<ElementDto>().Do();
        }

        if (!TableExists(Constants.DatabaseSchema.Tables.ElementVersion))
        {
            Create.Table<ElementVersionDto>().Do();
        }

        if (!TableExists(Constants.DatabaseSchema.Tables.ElementCultureVariation))
        {
            Create.Table<ElementCultureVariationDto>().Do();
        }
    }

    private void EnsureElementRecycleBin()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<NodeDto>(x => x.NodeId)
            .From<NodeDto>()
            .Where<NodeDto>(x => x.UniqueId == Constants.System.RecycleBinElementKey);

        if (Database.FirstOrDefault<NodeDto>(sql) is not null)
        {
            return;
        }

        ToggleIdentityInsertForNodes(true);
        try
        {
            Database.Insert(
                Constants.DatabaseSchema.Tables.Node,
                "id",
                false,
                new NodeDto
                {
                    NodeId = Constants.System.RecycleBinElement,
                    Trashed = false,
                    ParentId = -1,
                    UserId = -1,
                    Level = 0,
                    Path = "-1,-22",
                    SortOrder = 0,
                    UniqueId = Constants.System.RecycleBinElementKey,
                    Text = "Recycle Bin",
                    NodeObjectType = Constants.ObjectTypes.ElementRecycleBin,
                    CreateDate = DateTime.UtcNow,
                });
        }
        finally
        {
            ToggleIdentityInsertForNodes(false);
        }
    }

    private void ToggleIdentityInsertForNodes(bool toggleOn)
    {
        if (SqlSyntax.SupportsIdentityInsert())
        {
            Database.Execute(new Sql($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(NodeDto.TableName)} {(toggleOn ? "ON" : "OFF")} "));
        }
    }

    private void EnsureElementStartNodeColumn()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.UserGroup)
                             && x.ColumnName.InvariantEquals("startElementId")) == false)
        {
            AddColumn<UserGroupDto>(Constants.DatabaseSchema.Tables.UserGroup, "startElementId");
        }
    }
}
