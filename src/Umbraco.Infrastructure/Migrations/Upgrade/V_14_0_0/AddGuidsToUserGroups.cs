using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class AddGuidsToUserGroups : UnscopedMigrationBase
{
    private const string NewColumnName = "key";
    private readonly IScopeProvider _scopeProvider;

    public AddGuidsToUserGroups(IMigrationContext context, IScopeProvider scopeProvider)
        : base(context)
    {
        _scopeProvider = scopeProvider;
    }

    protected override void Migrate()
    {
        // If the new column already exists we'll do nothing.
        if (ColumnExists(Constants.DatabaseSchema.Tables.UserGroup, NewColumnName))
        {
            Context.Complete();
            return;
        }

        // SQL server can simply add the column, but for SQLite this won't work,
        // so we'll have to create a new table and copy over data.
        if (DatabaseType != DatabaseType.SQLite)
        {
            MigrateSqlServer();
            Context.Complete();
            return;
        }

        MigrateSqlite();
        Context.Complete();
    }

    private void MigrateSqlServer()
    {
        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
        AddColumnIfNotExists<UserGroupDto>(columns, NewColumnName);

        var nodeDtoTrashedIndex = $"IX_umbracoUserGroup_userGroupKey";
        CreateIndex<UserGroupDto>(nodeDtoTrashedIndex);

        // We want specific keys for the default user groups, so we need to fetch the user groups again to set their keys.
        List<UserGroupDto>? userGroups = Database.Fetch<UserGroupDto>();

        foreach (UserGroupDto userGroup in userGroups)
        {
            userGroup.Key = ResolveAliasToGuid(userGroup.Alias);
            Database.Update(userGroup);
        }

        Context.Complete();

        scope.Complete();
    }

    private void MigrateSqlite()
    {
        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        // This isn't pretty,
        // But since you cannot alter columns, we have to copy the data over and delete the old table.
        // However we cannot do this due to foreign keys, so temporarily disable these keys while migrating.
        // This leads to an interesting chicken and egg issue, we have to do this before a transaction
        // but in the same connection, since foreign keys will default to "ON" next time we open a connection.
        // This means we have to just execute the DB command to end the transaction,
        // instead of using CompleteTransaction, since this will end the connection.
        Database.Execute("COMMIT;");

        // We don't have to worry about re-enabling this since it happens automatically.
        Database.Execute("PRAGMA foreign_keys=off;");
        Database.Execute("BEGIN TRANSACTION;");

        // Now that keys are disabled and we have a transaction, we'll do our migration.
        MigrateColumnSqlite();

        Context.Complete();
        scope.Complete();
    }

    private void MigrateColumnSqlite()
    {
        IEnumerable<UserGroupDto> groups = Database.Fetch<OldUserGroupDto>().Select(x => new UserGroupDto
        {
            Id = x.Id,
            Key = ResolveAliasToGuid(x.Alias),
            Alias = x.Alias,
            Name = x.Name,
            DefaultPermissions = x.DefaultPermissions,
            CreateDate = x.CreateDate,
            UpdateDate = x.UpdateDate,
            Icon = x.Icon,
            HasAccessToAllLanguages = x.HasAccessToAllLanguages,
            StartContentId = x.StartContentId,
            StartMediaId = x.StartMediaId,
            UserGroup2AppDtos = x.UserGroup2AppDtos,
            UserGroup2LanguageDtos = x.UserGroup2LanguageDtos
        });

        // I realize that this may seem a bit drastic,
        // however, since SQLite cannot generate GUIDs we have to load all the user groups into memory to generate it.
        // So instead of going through the trouble of creating a new table, copying over data, and then deleting
        // We can just drop the table directly and re-create it to add the new column.
        Delete.Table(Constants.DatabaseSchema.Tables.UserGroup).Do();
        Create.Table<UserGroupDto>().Do();

        // We have to insert one at a time to be able to not auto increment the id.
        foreach (UserGroupDto group in groups)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false, group);
        }
    }

    private Guid ResolveAliasToGuid(string? alias)
        => alias switch
        {
            "admin" => Constants.Security.AdminGroupKey,
            "editor" => Constants.Security.EditorGroupKey,
            "sensitiveData" => Constants.Security.SensitiveDataGroupKey,
            "translator" => Constants.Security.TranslatorGroupKey,
            "writer" => Constants.Security.WriterGroupKey,
            _ => Guid.NewGuid(),
        };

    [TableName(Constants.DatabaseSchema.Tables.UserGroup)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    private class OldUserGroupDto
    {
        public OldUserGroupDto()
        {
            UserGroup2AppDtos = new List<UserGroup2AppDto>();
            UserGroup2LanguageDtos = new List<UserGroup2LanguageDto>();
        }

        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 6)]
        public int Id { get; set; }

        [Column("userGroupAlias")]
        [Length(200)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupAlias")]
        public string? Alias { get; set; }

        [Column("userGroupName")]
        [Length(200)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupName")]
        public string? Name { get; set; }

        [Column("userGroupDefaultPermissions")]
        [Length(50)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? DefaultPermissions { get; set; }

        [Column("createDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime CreateDate { get; set; }

        [Column("updateDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime UpdateDate { get; set; }

        [Column("icon")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? Icon { get; set; }

        [Column("hasAccessToAllLanguages")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool HasAccessToAllLanguages { get; set; }

        [Column("startContentId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(NodeDto), Name = "FK_startContentId_umbracoNode_id")]
        public int? StartContentId { get; set; }

        [Column("startMediaId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(NodeDto), Name = "FK_startMediaId_umbracoNode_id")]
        public int? StartMediaId { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
        public List<UserGroup2AppDto> UserGroup2AppDtos { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
        public List<UserGroup2LanguageDto> UserGroup2LanguageDtos { get; set; }

        /// <summary>
        ///     This is only relevant when this column is included in the results (i.e. GetUserGroupsWithUserCounts).
        /// </summary>
        [ResultColumn]
        public int UserCount { get; set; }
    }
}
