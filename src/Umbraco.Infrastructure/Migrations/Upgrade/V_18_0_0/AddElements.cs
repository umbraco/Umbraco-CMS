using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

public class AddElements : AsyncMigrationBase
{
    private readonly IRelationService _relationService;

    public AddElements(IMigrationContext context, IRelationService relationService)
        : base(context)
        => _relationService = relationService;

    protected override Task MigrateAsync()
    {
        EnsureElementTreeLock();
        EnsureElementTables();
        EnsureElementRecycleBin();
        EnsureElementStartNodeColumn();
        EnsureAdminGroupElementAccess();
        EnsureAdminGroupElementPermissions();
        EnsureElementRelationTypes();
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

    private void EnsureAdminGroupElementAccess()
    {
        // Set startElementId to -1 (root) for admin group if not already set
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Update<UserGroupDto>(u => u.Set(x => x.StartElementId, Constants.System.Root))
            .Where<UserGroupDto>(x => x.Key == Constants.Security.AdminGroupKey && x.StartElementId == null);

        Database.Execute(sql);
    }

    private void EnsureAdminGroupElementPermissions()
    {
        // Check if admin group already has any element permissions
        Sql<ISqlContext> existingPermissionsSql = Database.SqlContext.Sql()
            .Select<UserGroup2PermissionDto>()
            .From<UserGroup2PermissionDto>()
            .Where<UserGroup2PermissionDto>(x =>
                x.UserGroupKey == Constants.Security.AdminGroupKey &&
                x.Permission == ActionElementBrowse.ActionLetter);

        if (Database.Fetch<UserGroup2PermissionDto>(existingPermissionsSql).Count != 0)
        {
            return;
        }

        // Add all element permissions for admin group
        var elementPermissions = new[]
        {
            ActionElementNew.ActionLetter,
            ActionElementUpdate.ActionLetter,
            ActionElementDelete.ActionLetter,
            ActionElementMove.ActionLetter,
            ActionElementCopy.ActionLetter,
            ActionElementPublish.ActionLetter,
            ActionElementUnpublish.ActionLetter,
            ActionElementBrowse.ActionLetter,
            ActionElementRollback.ActionLetter,
        };

        UserGroup2PermissionDto[] permissionDtos = elementPermissions
            .Select(permission => new UserGroup2PermissionDto
            {
                UserGroupKey = Constants.Security.AdminGroupKey,
                Permission = permission,
            })
            .ToArray();

        Database.InsertBulk(permissionDtos);
    }

    private void EnsureElementRelationTypes()
    {
        EnsureRelationType(
            Constants.Conventions.RelationTypes.RelatedElementAlias,
            Constants.Conventions.RelationTypes.RelatedElementName,
            parentObjectType: null,
            childObjectType: null,
            isDependency: true);

        EnsureRelationType(
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnElementDeleteAlias,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnElementDeleteName,
            parentObjectType: Constants.ObjectTypes.ElementContainer,
            childObjectType: Constants.ObjectTypes.Element,
            isDependency: false);

        EnsureRelationType(
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnContainerDeleteAlias,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnContainerDeleteName,
            parentObjectType: Constants.ObjectTypes.ElementContainer,
            childObjectType: Constants.ObjectTypes.ElementContainer,
            isDependency: false);
    }

    private void EnsureRelationType(
        string alias,
        string name,
        Guid? parentObjectType,
        Guid? childObjectType,
        bool isDependency)
    {
        IRelationType? relationType = _relationService.GetRelationTypeByAlias(alias);
        if (relationType != null)
        {
            return;
        }

        // Generate a new unique relation type key that is the same as would have come from a new install.
        Guid key = DatabaseDataCreator.CreateUniqueRelationTypeId(alias, name);

        // Create new relation type using service, so the repository cache gets updated as well.
        relationType = new RelationType(name, alias, false, parentObjectType, childObjectType, isDependency)
        {
            Key = key
        };
        _relationService.Save(relationType);
    }
}
