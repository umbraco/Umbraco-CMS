using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    // This migration exists for 7.3.0 but it seems like it was not always running properly
    // if you're upgrading from 7.3.0 or higher than we add this migration, if you're upgrading 
    // from 7.3.0 or lower then you will already get this migration in the migration to get to 7.3.0
    [Migration("7.3.0", "7.5.0", 10, GlobalSettings.UmbracoMigrationName)]
    public class EnsureServersLockObject : MigrationBase
    {
        public EnsureServersLockObject(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            // that lock object should have been part of BaseDataCreation since 7.3.0 but
            // for some reason it was not, so it was created during migrations but not during
            // new installs, so for ppl that upgrade, make sure they have it

            EnsureLockObject(Constants.System.ServersLock, "0AF5E610-A310-4B6F-925F-E928D5416AF7", "LOCK: Servers");
        }

        public override void Down()
        {
            // not implemented
        }

        private void EnsureLockObject(int id, string uniqueId, string text)
        {
            var exists = Context.Database.Exists<NodeDto>(id);
            if (exists) return;

            Insert
                .IntoTable("umbracoNode")
                .EnableIdentityInsert()
                .Row(new
                {
                    id = id, // NodeId
                    trashed = false,
                    parentId = -1,
                    nodeUser = 0,
                    level = 1,
                    path = "-1," + id,
                    sortOrder = 0,
                    uniqueId = new Guid(uniqueId),
                    text = text,
                    nodeObjectType = new Guid(Constants.ObjectTypes.LockObject),
                    createDate = DateTime.Now
                });
        }
    }
}
