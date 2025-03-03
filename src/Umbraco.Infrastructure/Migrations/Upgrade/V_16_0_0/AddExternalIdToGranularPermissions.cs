using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_0_0;

// todo move to corescope?
public class AddExternalIdToGranularPermissions : UnscopedMigrationBase
{
    private readonly ICoreScopeProvider _scopeProvider;
    private const string NewColumnName = "umbracoExternalIds";
    private const string NewIndex = "IX_umbracoUserGroup2GranularPermissionDto_ExternalUniqueId";

    public AddExternalIdToGranularPermissions(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider)
        : base(context)
    {
        _scopeProvider = scopeProvider;
    }

    protected override void Migrate()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        if (ColumnExists(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission, NewColumnName) is false)
        {
            if (DatabaseType == DatabaseType.SQLite)
            {
                UpdateColumnSqlLite();
            }
            else
            {
                UpdateColumnSqlServer();
            }
        }

        if (IndexExists(NewIndex) is false)
        {
            CreateIndex<UserGroup2GranularPermissionDto>(NewIndex);
        }

        Context.Complete();
        scope.Complete();
    }

    protected void UpdateColumnSqlServer()
    {
        Alter.Table(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission)
            .AddColumn(NewColumnName)
            .AsGuid()
            .Nullable()
            .Do();
    }

    protected void UpdateColumnSqlLite()
    {
        var oldEntities = Database.Fetch<OldUserGroup2GranularPermissionDto>();
        var newEntities = oldEntities.Select(e => new UserGroup2GranularPermissionDto
        {
            Id = e.Id,
            Context = e.Context,
            Permission = e.Permission,
            UniqueId = e.UniqueId,
            UserGroupKey = e.UserGroupKey
        });

        Delete.Table(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission).Do();
        Create.Table<UserGroup2GranularPermissionDto>().Do();

        Database.InsertBulk(newEntities);
    }

    [TableName(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission)]
    private class OldUserGroup2GranularPermissionDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoUserGroup2GranularPermissionDto", AutoIncrement = true)]
        public int Id { get; set; }

        [Column("userGroupKey")]
        [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserGroup2GranularPermissionDto_UserGroupKey_UniqueId",
            IncludeColumns = "uniqueId")]
        [ForeignKey(typeof(UserGroupDto), Column = "key")]
        public Guid UserGroupKey { get; set; }

        [Column("uniqueId")]
        [ForeignKey(typeof(NodeDto), Column = "uniqueId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserGroup2GranularPermissionDto_UniqueId")]
        public Guid? UniqueId { get; set; }

        [Column("permission")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public required string Permission { get; set; }

        [Column("context")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public required string Context { get; set; }
    }
}
