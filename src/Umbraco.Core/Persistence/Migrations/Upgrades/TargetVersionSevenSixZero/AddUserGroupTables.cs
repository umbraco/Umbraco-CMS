using System;
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
            var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();

            AddNewTables(tables);
            MigrateUserPermissions();
            MigrateUserTypesToGroups();
            DeleteOldTables(tables, constraints);
        }

        private void AddNewTables(string[] tables)
        {
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
                    .Columns(new[] {"userId", "userGroupId"});
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
                    .Columns(new[] {"userGroupId", "app"});
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
                    .Columns(new[] {"userGroupId", "nodeId", "permission"});
                Create.ForeignKey("FK_umbracoUserGroup2NodePermission_umbracoNode_id")
                    .FromTable("umbracoUserGroup2NodePermission").ForeignColumn("nodeId")
                    .ToTable("umbracoNode").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
                Create.ForeignKey("FK_umbracoUserGroup2NodePermission_umbracoUserGroup_id")
                    .FromTable("umbracoUserGroup2NodePermission").ForeignColumn("userGroupId")
                    .ToTable("umbracoUserGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
            }
        }

        private void MigrateUserTypesToGroups()
        {
            // TODO: review for MySQL and CE (tested only on SQL Express)

            // Create a user group for each user type
            Execute.Sql(@"INSERT INTO umbracoUserGroup (userGroupAlias, userGroupName, userGroupDefaultPermissions)
                SELECT userTypeAlias, userTypeName, userTypeDefaultPermissions
                FROM umbracoUserType");

            // Add each user to the group created from their type
            Execute.Sql(@"INSERT INTO umbracoUser2UserGroup (userId, userGroupId)
                SELECT u.id,(
	                SELECT ug.id
	                FROM umbracoUserGroup ug
	                INNER JOIN umbracoUserType ut ON ut.userTypeAlias = ug.userGroupAlias
	                WHERE u.userType = ut.id
                )
                FROM umbracoUser u");

            // Add the built-in administrator account to all apps
            Execute.Sql(@"INSERT INTO umbracoUserGroup2app (userGroupId,app)
                SELECT ug.id, app
                FROM umbracoUserGroup ug
                INNER JOIN umbracoUser2UserGroup u2ug ON u2ug.userGroupId = ug.id
                INNER JOIN umbracoUser u ON u.id = u2ug.userId
                INNER JOIN umbracoUser2app u2a ON u2a.[user] = u.id
				WHERE u.id = 0");

            // Rename some groups for consistency (plural form)
            Execute.Sql("UPDATE umbracoUserGroup SET userGroupName = 'Writers' WHERE userGroupName = 'Writer'");
            Execute.Sql("UPDATE umbracoUserGroup SET userGroupName = 'Translators' WHERE userGroupName = 'Translator'");
        }

        private void MigrateUserPermissions()
        {
            // TODO: review for MySQL and CE (tested only on SQL Express)

            // Create user group records for all non-admin users that have specific permissions set
            Execute.Sql(@"INSERT INTO umbracoUserGroup(userGroupAlias, userGroupName)
                SELECT userName + 'Group', 'Group for ' + userName
                FROM umbracoUser
                WHERE (id IN (
	                SELECT [user]
	                FROM umbracoUser2app
                ) OR id IN (
	                SELECT userid
	                FROM umbracoUser2NodePermission
                ))
                AND id > 0");

            // Associate those groups with the users
            Execute.Sql(@"INSERT INTO umbracoUser2UserGroup (userId, userGroupId)
                SELECT u.id, ug.id
                FROM umbracoUser u
                INNER JOIN umbracoUserGroup ug ON ug.userGroupAlias = userName + 'Group'");

            // Create node permissions on the groups
            Execute.Sql(@"INSERT INTO umbracoUserGroup2NodePermission (userGroupId,nodeId,permission)
                SELECT ug.id, nodeId, permission
                FROM umbracoUserGroup ug
                INNER JOIN umbracoUser2UserGroup u2ug ON u2ug.userGroupId = ug.id
                INNER JOIN umbracoUser u ON u.id = u2ug.userId
                INNER JOIN umbracoUser2NodePermission u2np ON u2np.userId = u.id
				WHERE ug.userGroupAlias NOT IN (
					SELECT userTypeAlias
					FROM umbracoUserType
				)");

            // Create app permissions on the groups
            Execute.Sql(@"INSERT INTO umbracoUserGroup2app (userGroupId,app)
                SELECT ug.id, app
                FROM umbracoUserGroup ug
                INNER JOIN umbracoUser2UserGroup u2ug ON u2ug.userGroupId = ug.id
                INNER JOIN umbracoUser u ON u.id = u2ug.userId
                INNER JOIN umbracoUser2app u2a ON u2a.[user] = u.id
				WHERE ug.userGroupAlias NOT IN (
					SELECT userTypeAlias
					FROM umbracoUserType
				)");
        }

        private void DeleteOldTables(string[] tables, Tuple<string, string, string>[] constraints)
        {
            if (tables.InvariantContains("umbracoUser2App"))
            {
                Delete.Table("umbracoUser2App");
            }

            if (tables.InvariantContains("umbracoUser2NodePermission"))
            {
                Delete.Table("umbracoUser2NodePermission");
            }

            if (tables.InvariantContains("umbracoUserType") && tables.InvariantContains("umbracoUser"))
            {
                if (constraints.Any(x => x.Item1.InvariantEquals("umbracoUser") && x.Item3.InvariantEquals("FK_umbracoUser_umbracoUserType_id")))
                { 
                    Delete.ForeignKey("FK_umbracoUser_umbracoUserType_id").OnTable("umbracoUser");
                }

                Delete.Column("userType").FromTable("umbracoUser");
                Delete.Table("umbracoUserType");
            }
        }

        public override void Down()
        { }
    }
}