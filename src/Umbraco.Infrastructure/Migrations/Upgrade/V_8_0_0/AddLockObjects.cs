using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddLockObjects : MigrationBase
    {
        public AddLockObjects(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // some may already exist, just ensure everything we need is here
            EnsureLockObject(Cms.Core.Constants.Locks.Servers, "Servers");
            EnsureLockObject(Cms.Core.Constants.Locks.ContentTypes, "ContentTypes");
            EnsureLockObject(Cms.Core.Constants.Locks.ContentTree, "ContentTree");
            EnsureLockObject(Cms.Core.Constants.Locks.MediaTree, "MediaTree");
            EnsureLockObject(Cms.Core.Constants.Locks.MemberTree, "MemberTree");
            EnsureLockObject(Cms.Core.Constants.Locks.MediaTypes, "MediaTypes");
            EnsureLockObject(Cms.Core.Constants.Locks.MemberTypes, "MemberTypes");
            EnsureLockObject(Cms.Core.Constants.Locks.Domains, "Domains");
        }

        private void EnsureLockObject(int id, string name)
        {
            EnsureLockObject(Database, id, name);
        }

        internal static void EnsureLockObject(IUmbracoDatabase db, int id, string name)
        {
            // not if it already exists
            var exists = db.Exists<LockDto>(id);
            if (exists) return;

            // be safe: delete old umbracoNode lock objects if any
            db.Execute($"DELETE FROM umbracoNode WHERE id={id};");

            // then create umbracoLock object
            db.Execute($"INSERT umbracoLock (id, name, value) VALUES ({id}, '{name}', 1);");
        }
    }
}
