using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using ColumnInfo = Umbraco.Core.Persistence.SqlSyntax.ColumnInfo;

namespace Umbraco.Core.Migrations.Upgrade.V_7_7_0
{
    public class AddUserGroupTables : MigrationBase
    {
        private readonly string _collateSyntax;

        public AddUserGroupTables(IMigrationContext context)
            : base(context)
        {
            //For some of the migration data inserts we require to use a special MSSQL collate expression since
            //some databases may have a custom collation specified and if that is the case, when we compare strings
            //in dynamic SQL it will try to compare strings in different collations and this will yield errors.
            _collateSyntax = "COLLATE DATABASE_DEFAULT";
        }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToList();
            var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            //In some very rare cases, there might already be user group tables that we'll need to remove first
            //but of course we don't want to remove the tables we will be creating below if they already exist so
            //need to do some checks first since these old rare tables have a different schema
            RemoveOldTablesIfExist(tables, columns);

            if (AddNewTables(tables))
            {
                MigrateUserPermissions();
                MigrateUserTypesToGroups();
                DeleteOldTables(tables, constraints);
                SetDefaultIcons();
            }
            else
            {
                //if we aren't adding the tables, make sure that the umbracoUserGroup table has the correct FKs - these
                //were added after the beta release so we need to do some cleanup
                //if the FK doesn't exist
                if (constraints.Any(x => x.Item1.InvariantEquals("umbracoUserGroup")
                    && x.Item2.InvariantEquals("startContentId")
                    && x.Item3.InvariantEquals("FK_startContentId_umbracoNode_id")) == false)
                {
                    //before we add any foreign key we need to make sure there's no stale data in there which would  have happened in the beta
                    //release if a start node was assigned and then that start node was deleted.
                    Database.Execute(@"UPDATE umbracoUserGroup SET startContentId = NULL WHERE startContentId NOT IN (SELECT id FROM umbracoNode)");

                    Create.ForeignKey("FK_startContentId_umbracoNode_id")
                        .FromTable("umbracoUserGroup")
                        .ForeignColumn("startContentId")
                        .ToTable("umbracoNode")
                        .PrimaryColumn("id")
                        .OnDelete(Rule.None)
                        .OnUpdate(Rule.None)
                        .Do();
                }

                if (constraints.Any(x => x.Item1.InvariantEquals("umbracoUserGroup")
                    && x.Item2.InvariantEquals("startMediaId")
                    && x.Item3.InvariantEquals("FK_startMediaId_umbracoNode_id")) == false)
                {
                    //before we add any foreign key we need to make sure there's no stale data in there which would  have happened in the beta
                    //release if a start node was assigned and then that start node was deleted.
                    Database.Execute(@"UPDATE umbracoUserGroup SET startMediaId = NULL WHERE startMediaId NOT IN (SELECT id FROM umbracoNode)");

                    Create.ForeignKey("FK_startMediaId_umbracoNode_id")
                        .FromTable("umbracoUserGroup")
                        .ForeignColumn("startMediaId")
                        .ToTable("umbracoNode")
                        .PrimaryColumn("id")
                        .OnDelete(Rule.None)
                        .OnUpdate(Rule.None)
                        .Do();
                }
            }
        }

        /// <summary>
        /// In some very rare cases, there might already be user group tables that we'll need to remove first
        /// but of course we don't want to remove the tables we will be creating below if they already exist so
        /// need to do some checks first since these old rare tables have a different schema
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="columns"></param>
        private void RemoveOldTablesIfExist(List<string> tables, ColumnInfo[] columns)
        {
            if (tables.Contains("umbracoUser2userGroup", StringComparer.InvariantCultureIgnoreCase))
            {
                //this column doesn't exist in the 7.7 schema, so if it's there, then this is a super old table
                var foundOldColumn = columns
                    .FirstOrDefault(x =>
                        x.ColumnName.Equals("user", StringComparison.InvariantCultureIgnoreCase)
                        && x.TableName.Equals("umbracoUser2userGroup", StringComparison.InvariantCultureIgnoreCase));
                if (foundOldColumn != null)
                {
                    Delete.Table("umbracoUser2userGroup").Do();
                    //remove from the tables list since this will be re-checked in further logic
                    tables.Remove("umbracoUser2userGroup");
                }
            }

            if (tables.Contains("umbracoUserGroup", StringComparer.InvariantCultureIgnoreCase))
            {
                //The new schema has several columns, the super old one for this table only had 2 so if it's 2 get rid of it
                var countOfCols = columns
                    .Count(x => x.TableName.Equals("umbracoUserGroup", StringComparison.InvariantCultureIgnoreCase));
                if (countOfCols == 2)
                {
                    Delete.Table("umbracoUserGroup").Do();
                    //remove from the tables list since this will be re-checked in further logic
                    tables.Remove("umbracoUserGroup");
                }
            }
        }

        private void SetDefaultIcons()
        {
            Database.Execute($"UPDATE umbracoUserGroup SET icon = \'\' WHERE userGroupAlias = \'{Constants.Security.AdminGroupAlias}\'");
            Database.Execute("UPDATE umbracoUserGroup SET icon = \'icon-edit\' WHERE userGroupAlias = \'writer\'");
            Database.Execute("UPDATE umbracoUserGroup SET icon = \'icon-tools\' WHERE userGroupAlias = \'editor\'");
            Database.Execute("UPDATE umbracoUserGroup SET icon = \'icon-globe\' WHERE userGroupAlias = \'translator\'");
        }

        private bool AddNewTables(List<string> tables)
        {
            var updated = false;
            if (tables.InvariantContains("umbracoUserGroup") == false)
            {
                Create.Table<UserGroupDto>().Do();
                updated = true;
            }

            if (tables.InvariantContains("umbracoUser2UserGroup") == false)
            {
                Create.Table<User2UserGroupDto>().Do();
                updated = true;
            }

            if (tables.InvariantContains("umbracoUserGroup2App") == false)
            {
                Create.Table<UserGroup2AppDto>().Do();
                updated = true;
            }

            if (tables.InvariantContains("umbracoUserGroup2NodePermission") == false)
            {
                Create.Table<UserGroup2NodePermissionDto>().Do();
                updated = true;
            }

            return updated;
        }

        private void MigrateUserTypesToGroups()
        {
            // Create a user group for each user type
            Database.Execute(@"INSERT INTO umbracoUserGroup (userGroupAlias, userGroupName, userGroupDefaultPermissions)
                SELECT userTypeAlias, userTypeName, userTypeDefaultPermissions
                FROM umbracoUserType");

            // Add each user to the group created from their type
            Database.Execute(string.Format(@"INSERT INTO umbracoUser2UserGroup (userId, userGroupId)
                SELECT u.id, ug.id
                FROM umbracoUser u
                INNER JOIN umbracoUserType ut ON ut.id = u.userType
                INNER JOIN umbracoUserGroup ug ON ug.userGroupAlias {0} = ut.userTypeAlias {0}", _collateSyntax));

            // Add the built-in administrator account to all apps
            // this will lookup all of the apps that the admin currently has access to in order to assign the sections
            // instead of use statically assigning since there could be extra sections we don't know about.
            Database.Execute(@"INSERT INTO umbracoUserGroup2app (userGroupId,app)
                SELECT ug.id, app
                FROM umbracoUserGroup ug
                INNER JOIN umbracoUser2UserGroup u2ug ON u2ug.userGroupId = ug.id
                INNER JOIN umbracoUser u ON u.id = u2ug.userId
                INNER JOIN umbracoUser2app u2a ON u2a." + SqlSyntax.GetQuotedColumnName("user") + @" = u.id
                WHERE u.id = 0");

            // Add the default section access to the other built-in accounts
            //  writer:
            Database.Execute(string.Format(@"INSERT INTO umbracoUserGroup2app (userGroupId, app)
                SELECT ug.id, 'content' as app
                FROM umbracoUserGroup ug
                WHERE ug.userGroupAlias {0} = 'writer' {0}", _collateSyntax));
            //  editor
            Database.Execute(string.Format(@"INSERT INTO umbracoUserGroup2app (userGroupId, app)
                SELECT ug.id, 'content' as app
                FROM umbracoUserGroup ug
                WHERE ug.userGroupAlias {0} = 'editor' {0}", _collateSyntax));
            Database.Execute(string.Format(@"INSERT INTO umbracoUserGroup2app (userGroupId, app)
                SELECT ug.id, 'media' as app
                FROM umbracoUserGroup ug
                WHERE ug.userGroupAlias {0} = 'editor' {0}", _collateSyntax));
            //  translator
            Database.Execute(string.Format(@"INSERT INTO umbracoUserGroup2app (userGroupId, app)
                SELECT ug.id, 'translation' as app
                FROM umbracoUserGroup ug
                WHERE ug.userGroupAlias {0} = 'translator' {0}", _collateSyntax));

            //We need to lookup all distinct combinations of section access and create a group for each distinct collection
            //and assign groups accordingly. We'll perform the lookup 'now' to then create the queued SQL migrations.
            var userAppsData = Context.Database.Query<dynamic>(@"SELECT u.id, u2a.app FROM umbracoUser u
                                INNER JOIN umbracoUser2app u2a ON u2a." + SqlSyntax.GetQuotedColumnName("user") + @" = u.id
                                ORDER BY u.id, u2a.app");
            var usersWithApps = new Dictionary<int, List<string>>();
            foreach (var userApps in userAppsData)
            {
                if (usersWithApps.TryGetValue(userApps.id, out List<string> apps) == false)
                {
                    apps = new List<string> {userApps.app};
                    usersWithApps.Add(userApps.id, apps);
                }
                else
                {
                    apps.Add(userApps.app);
                }
            }
            //At this stage we have a dictionary of users with a collection of their apps which are sorted
            //and we need to determine the unique/distinct app collections for each user to create groups with.
            //We can do this by creating a hash value of all of the app values and since they are already sorted we can get a distinct
            //collection by this hash.
            var distinctApps = usersWithApps
                .Select(x => new {appCollection = x.Value, appsHash = string.Join("", x.Value).GenerateHash()})
                .DistinctBy(x => x.appsHash)
                .ToArray();
            //Now we need to create user groups for each of these distinct app collections, and then assign the corresponding users to those groups
            for (var i = 0; i < distinctApps.Length; i++)
            {
                //create the group
                var alias = "MigratedSectionAccessGroup_" + (i + 1);
                Insert.IntoTable("umbracoUserGroup").Row(new
                {
                    userGroupAlias = "MigratedSectionAccessGroup_" + (i + 1),
                    userGroupName = "Migrated Section Access Group " + (i + 1)
                }).Do();
                //now assign the apps
                var distinctApp = distinctApps[i];
                foreach (var app in distinctApp.appCollection)
                {
                    Database.Execute(string.Format(@"INSERT INTO umbracoUserGroup2app (userGroupId, app)
                        SELECT ug.id, '" + app + @"' as app
                        FROM umbracoUserGroup ug
                        WHERE ug.userGroupAlias {0} = '" + alias + "' {0}", _collateSyntax));
                }
                //now assign the corresponding users to this group
                foreach (var userWithApps in usersWithApps)
                {
                    //check if this user's groups hash matches the current groups hash
                    var hash = string.Join("", userWithApps.Value).GenerateHash();
                    if (hash == distinctApp.appsHash)
                    {
                        //it matches so assign the user to this group
                        Database.Execute(string.Format(@"INSERT INTO umbracoUser2UserGroup (userId, userGroupId)
                            SELECT " + userWithApps.Key + @", ug.id
                            FROM umbracoUserGroup ug
                            WHERE ug.userGroupAlias {0} = '" + alias + "' {0}", _collateSyntax));
                    }
                }
            }

            // Rename some groups for consistency (plural form)
            Database.Execute("UPDATE umbracoUserGroup SET userGroupName = 'Writers' WHERE userGroupAlias = 'writer'");
            Database.Execute("UPDATE umbracoUserGroup SET userGroupName = 'Translators' WHERE userGroupAlias = 'translator'");

            //Ensure all built in groups have a start node of -1
            Database.Execute("UPDATE umbracoUserGroup SET startContentId = -1 WHERE userGroupAlias = 'editor'");
            Database.Execute("UPDATE umbracoUserGroup SET startMediaId = -1 WHERE userGroupAlias = 'editor'");
            Database.Execute("UPDATE umbracoUserGroup SET startContentId = -1 WHERE userGroupAlias = 'writer'");
            Database.Execute("UPDATE umbracoUserGroup SET startMediaId = -1 WHERE userGroupAlias = 'writer'");
            Database.Execute("UPDATE umbracoUserGroup SET startContentId = -1 WHERE userGroupAlias = 'translator'");
            Database.Execute("UPDATE umbracoUserGroup SET startMediaId = -1 WHERE userGroupAlias = 'translator'");
            Database.Execute("UPDATE umbracoUserGroup SET startContentId = -1 WHERE userGroupAlias = 'admin'");
            Database.Execute("UPDATE umbracoUserGroup SET startMediaId = -1 WHERE userGroupAlias = 'admin'");
        }

        private void MigrateUserPermissions()
        {
            // Create user group records for all non-admin users that have specific permissions set
            Database.Execute(@"INSERT INTO umbracoUserGroup(userGroupAlias, userGroupName)
                SELECT 'permissionGroupFor' + userLogin, 'Migrated Permission Group for ' + userLogin
                FROM umbracoUser
                WHERE (id IN (
                    SELECT userid
                    FROM umbracoUser2NodePermission
                ))
                AND id > 0");

            // Associate those groups with the users
            Database.Execute(string.Format(@"INSERT INTO umbracoUser2UserGroup (userId, userGroupId)
                SELECT u.id, ug.id
                FROM umbracoUser u
                INNER JOIN umbracoUserGroup ug ON ug.userGroupAlias {0} = 'permissionGroupFor' + userLogin {0}", _collateSyntax));

            // Create node permissions on the groups
            Database.Execute(string.Format(@"INSERT INTO umbracoUserGroup2NodePermission (userGroupId,nodeId,permission)
                SELECT ug.id, nodeId, permission
                FROM umbracoUserGroup ug
                INNER JOIN umbracoUser2UserGroup u2ug ON u2ug.userGroupId = ug.id
                INNER JOIN umbracoUser u ON u.id = u2ug.userId
                INNER JOIN umbracoUser2NodePermission u2np ON u2np.userId = u.id
                WHERE ug.userGroupAlias {0} NOT IN (
                    SELECT userTypeAlias {0}
                    FROM umbracoUserType
                )", _collateSyntax));

            // Create app permissions on the groups
            Database.Execute(string.Format(@"INSERT INTO umbracoUserGroup2app (userGroupId,app)
                SELECT ug.id, app
                FROM umbracoUserGroup ug
                INNER JOIN umbracoUser2UserGroup u2ug ON u2ug.userGroupId = ug.id
                INNER JOIN umbracoUser u ON u.id = u2ug.userId
                INNER JOIN umbracoUser2app u2a ON u2a." + SqlSyntax.GetQuotedColumnName("user") + @" = u.id
                WHERE ug.userGroupAlias {0} NOT IN (
                    SELECT userTypeAlias {0}
                    FROM umbracoUserType
                )", _collateSyntax));
        }

        private void DeleteOldTables(List<string> tables, Tuple<string, string, string>[] constraints)
        {
            if (tables.InvariantContains("umbracoUser2App"))
            {
                Delete.Table("umbracoUser2App").Do();
            }

            if (tables.InvariantContains("umbracoUser2NodePermission"))
            {
                Delete.Table("umbracoUser2NodePermission").Do();
            }

            if (tables.InvariantContains("umbracoUserType") && tables.InvariantContains("umbracoUser"))
            {
                //Delete the FK if it exists before dropping the column
                if (constraints.Any(x => x.Item1.InvariantEquals("umbracoUser") && x.Item3.InvariantEquals("FK_umbracoUser_umbracoUserType_id")))
                {
                    Delete.ForeignKey("FK_umbracoUser_umbracoUserType_id").OnTable("umbracoUser").Do();
                }

                //This is the super old constraint name of the FK for user type so check this one too
                if (constraints.Any(x => x.Item1.InvariantEquals("umbracoUser") && x.Item3.InvariantEquals("FK_user_userType")))
                {
                    Delete.ForeignKey("FK_user_userType").OnTable("umbracoUser").Do();
                }

                Delete.Column("userType").FromTable("umbracoUser").Do();
                Delete.Table("umbracoUserType").Do();
            }
        }
    }
}
