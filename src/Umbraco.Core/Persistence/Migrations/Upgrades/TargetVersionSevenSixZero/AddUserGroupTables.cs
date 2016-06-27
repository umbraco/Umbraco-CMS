using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;
using System.Data;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 100, GlobalSettings.UmbracoMigrationName)]
    public class AddUserGroupTables : MigrationBase
    {
        public AddUserGroupTables(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoUserGroup") == false)
            {
                Create.Table("umbracoUserGroup")
                    .WithColumn("id").AsInt32().Identity().PrimaryKey("PK_umbracoUserGroup")
                    .WithColumn("userGroupAlias").AsString(200).NotNullable()
                    .WithColumn("userGroupName").AsString(200).NotNullable()
                    .WithColumn("userGroupDefaultPermissions").AsString(50).Nullable();
            }

            if (tables.InvariantContains("umbracoUser2UserGroup") == false)
            {
                Create.Table("umbracoUser2UserGroup")
                    .WithColumn("userId").AsInt32().NotNullable()
                    .WithColumn("userGroupId").AsInt32().NotNullable();
                Create.PrimaryKey("PK_user2userGroup")
                    .OnTable("umbracoUser2UserGroup")
                    .Columns(new[] { "userId", "userGroupId" });
                Create.ForeignKey("FK_umbracoUser2UserGroup_userId")
                    .FromTable("umbracoUser2UserGroup").ForeignColumn("userId")
                    .ToTable("umbracoUser").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
                Create.ForeignKey("FK_umbracoUser2UserGroup_userGroupId")
                    .FromTable("umbracoUser2UserGroup").ForeignColumn("userGroupId")
                    .ToTable("umbracoUserGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
            }

            if (tables.InvariantContains("umbracoUserGroup2App") == false)
            {
                Create.Table("umbracoUserGroup2App")
                    .WithColumn("userGroupId").AsInt32().NotNullable()
                    .WithColumn("app").AsString(50).NotNullable();
                Create.PrimaryKey("PK_userGroup2App")
                    .OnTable("umbracoUserGroup2App")
                    .Columns(new[] { "userGroupId", "app" });
                Create.ForeignKey("FK_umbracoUserGroup2App_umbracoGroupUser_id")
                    .FromTable("umbracoUserGroup2App").ForeignColumn("userGroupId")
                    .ToTable("umbracoUserGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
            }

            if (tables.InvariantContains("umbracoUserGroup2NodePermission") == false)
            {
                Create.Table("umbracoUserGroup2NodePermission")
                    .WithColumn("userGroupId").AsInt32().NotNullable()
                    .WithColumn("nodeId").AsInt32().NotNullable()
                    .WithColumn("permission").AsString(10).NotNullable();
                Create.PrimaryKey("PK_umbracoUserGroup2NodePermission")
                    .OnTable("umbracoUserGroup2NodePermission")
                    .Columns(new[] { "userGroupId", "nodeId", "permission" });
                Create.ForeignKey("FK_umbracoUserGroup2NodePermission_umbracoNode_id")
                    .FromTable("umbracoUserGroup2NodePermission").ForeignColumn("nodeId")
                    .ToTable("umbracoNode").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
                Create.ForeignKey("FK_umbracoUserGroup2NodePermission_umbracoUserGroup_id")
                    .FromTable("umbracoUserGroup2NodePermission").ForeignColumn("userGroupId")
                    .ToTable("umbracoUserGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
            }
        }

        public override void Down()
        { }
    }
}
