using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class AddGuidsToUserGroups : MigrationBase
{
    private const string OldTableName = $"{Constants.DatabaseSchema.Tables.UserGroup}UmbracoThirteenZeroGuid";

    public AddGuidsToUserGroups(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        // SQL server can simply add the column, but for SQLite this won't work,
        // so we'll have to create a new table and copy over data.
        if (DatabaseType != DatabaseType.SQLite)
        {
            AddColumnIfNotExists<UserGroupDto>(columns, "uniqueId");
        }

        // Rename the table to something that's unlikely to conflict with custom tables
        var tableName = Constants.DatabaseSchema.Tables.UserGroup;
        Rename
            .Table(Constants.DatabaseSchema.Tables.UserGroup)
            .To(OldTableName)
            .Do();

        // GetDefinedIndexes returns indexes as a tuple (TableName, IndexName, ColumnName, IsUnique)
        IEnumerable<string> indexNames = SqlSyntax.GetDefinedIndexes(Database)
            .Where(x => x.Item1 == OldTableName)
            .Select(x => x.Item2);

        // We have to delete the existing indexes to be able to recreate the table since that also creates the indexes.
        foreach (var indexName in indexNames)
        {
            Delete
                .Index(indexName)
                .OnTable(OldTableName)
                .Do();
        }

        // Now we can create the table again with all the right columns and then migrate the data
        Create.Table<UserGroupDto>().Do();

        // SQLite doesn't really seem to support defaulting some columns, so we'll have to fetch all and then re-save.
        IEnumerable<UserGroupDto> groups = Database.Fetch<OldUserGroupDto>().Select(x => new UserGroupDto
        {
            Id = x.Id,
            UniqueId = Guid.NewGuid(),
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

        // We have to insert one at a time to be able to not auto increment id
        foreach (UserGroupDto group in groups)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false, group);
        }

        Delete
            .ForeignKey("FK_startContentId_umbracoNode_id")
            .OnTable(OldTableName)
            .Do();

        Delete
            .ForeignKey("FK_startMediaId_umbracoNode_id")
            .OnTable(OldTableName)
            .Do();
        // // Finally we can remove the old table.
        Delete.Table(OldTableName).Do();
    }

    [TableName(OldTableName)]
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
        ///     This is only relevant when this column is included in the results (i.e. GetUserGroupsWithUserCounts)
        /// </summary>
        [ResultColumn]
        public int UserCount { get; set; }
    }
}
