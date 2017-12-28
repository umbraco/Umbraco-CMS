using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_6_0
{
    public class AddLockObjects : MigrationBase
    {
        public AddLockObjects(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            EnsureLockObject(Constants.Locks.Servers, "Servers");
        }

        private void EnsureLockObject(int id, string name)
        {
            var db = Database;
            var exists = db.Exists<LockDto>(id);
            if (exists) return;
            // be safe: delete old umbracoNode lock objects if any
            db.Execute("DELETE FROM umbracoNode WHERE id=@id;", new { id });
            // then create umbracoLock object
            db.Execute("INSERT umbracoLock (id, name, value) VALUES (@id, @name, 1);", new { id, name });
        }
    }
}
