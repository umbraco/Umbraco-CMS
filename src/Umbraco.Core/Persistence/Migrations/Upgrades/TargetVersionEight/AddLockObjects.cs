using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionEight
{
    [Migration("8.0.0", 101, GlobalSettings.UmbracoMigrationName)]
    public class AddLockObjects : MigrationBase
    {
        public AddLockObjects(IMigrationContext context)
            : base(context)
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
            Execute.Code(db =>
            {
                var exists = db.Exists<LockDto>(id);
                if (exists) return string.Empty;
                // be safe: delete old umbracoNode lock objects if any
                db.Execute($"DELETE FROM umbracoNode WHERE id={id};");
                // then create umbracoLock object
                db.Execute($"INSERT umbracoLock (id, name, value) VALUES ({id}, '{name}', 1);");
                return string.Empty;
            });
        }
    }
}
