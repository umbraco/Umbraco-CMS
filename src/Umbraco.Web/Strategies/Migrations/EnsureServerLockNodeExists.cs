using System;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Web.Strategies.Migrations
{
    public class EnsureServerLockNodeExists : MigrationStartupHander
    {
        protected override void AfterMigration(MigrationRunner sender, MigrationEventArgs e)
        {
            var target = new SemVersion(7, 3);

            if (e.ConfiguredSemVersion > target)
                return;

            var context = e.MigrationContext;
            var sqlSyntax = SqlSyntaxContext.SqlSyntaxProvider;

            // wrap in a transaction so that everything runs on the same connection
            // and the IDENTITY_INSERT stuff is effective for all inserts.
            using (var tr = context.Database.GetTransaction())
            {
                // turn on identity insert if db provider is not mysql
                if (sqlSyntax.SupportsIdentityInsert())
                    context.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON", sqlSyntax.GetQuotedTableName("umbracoNode"))));

                EnsureLockNode(context, Constants.System.ServersLock, "0AF5E610-A310-4B6F-925F-E928D5416AF7", "LOCK: Servers");

                // turn off identity insert if db provider is not mysql
                if (sqlSyntax.SupportsIdentityInsert())
                    context.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF", sqlSyntax.GetQuotedTableName("umbracoNode"))));

                tr.Complete();
            }
        }

        private static void EnsureLockNode(IMigrationContext context, int id, string uniqueId, string text)
        {
            var exists = context.Database.Exists<NodeDto>(id);
            if (exists) return;

            context.Database.Insert("umbracoNode", "id", false, new NodeDto
            {
                NodeId = id,
                Trashed = false,
                ParentId = -1,
                UserId = 0,
                Level = 1,
                Path = "-1," + id,
                SortOrder = 0,
                UniqueId = new Guid(uniqueId),
                Text = text,
                NodeObjectType = new Guid(Constants.ObjectTypes.LockObject),
                CreateDate = DateTime.Now
            });
        }
    }
}
