using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionEight
{
    [Migration("8.0.0", 100, GlobalSettings.UmbracoMigrationName)]
    public class AddLockObjects : MigrationBase
    {
        public AddLockObjects(ILogger logger)
            : base(logger)
        { }

        public override void Up()
        {
            // some may already exist, just ensure everything we need is here
            EnsureLockObject(Constants.Locks.Servers, "Servers");
            EnsureLockObject(Constants.Locks.ContentTypes, "ContentTypes");
            EnsureLockObject(Constants.Locks.ContentTree, "ContentTree");
            EnsureLockObject(Constants.Locks.MediaTree, "MediaTree");
            EnsureLockObject(Constants.Locks.MemberTree, "MemberTree");
            EnsureLockObject(Constants.Locks.MediaTypes, "MediaTypes");
            EnsureLockObject(Constants.Locks.MemberTypes, "MemberTypes");
            EnsureLockObject(Constants.Locks.Domains, "Domains");
        }

        public override void Down()
        {
            // not implemented
        }

        private void EnsureLockObject(int id, string name)
        {
            var exists = Context.Database.Exists<LockDto>(id);
            if (exists) return;

            // be safe: delete old umbracoNode lock objects if any
            Delete
                .FromTable("umbracoNode")
                .Row(new { id });

            Insert
                .IntoTable("umbracoLock")
                .EnableIdentityInsert()
                .Row(new { id, name });
        }
    }
}
