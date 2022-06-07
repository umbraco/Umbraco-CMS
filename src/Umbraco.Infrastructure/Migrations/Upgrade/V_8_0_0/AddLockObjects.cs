using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class AddLockObjects : MigrationBase
{
    public AddLockObjects(IMigrationContext context)
        : base(context)
    {
    }

    internal static void EnsureLockObject(IUmbracoDatabase db, int id, string name)
    {
        // not if it already exists
        var exists = db.Exists<LockDto>(id);
        if (exists)
        {
            return;
        }

        // be safe: delete old umbracoNode lock objects if any
        db.Execute($"DELETE FROM umbracoNode WHERE id={id};");

        // then create umbracoLock object
        db.Execute($"INSERT umbracoLock (id, name, value) VALUES ({id}, '{name}', 1);");
    }

    protected override void Migrate()
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

    private void EnsureLockObject(int id, string name) => EnsureLockObject(Database, id, name);
}
