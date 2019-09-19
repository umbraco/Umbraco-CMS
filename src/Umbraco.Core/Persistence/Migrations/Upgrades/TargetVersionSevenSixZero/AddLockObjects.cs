using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 101, Constants.System.UmbracoMigrationName)]
    public class AddLockObjects : MigrationBase
    {
        public AddLockObjects(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            EnsureLockObject(Constants.Locks.Servers, "Servers");
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
                db.Execute("DELETE FROM umbracoNode WHERE id=@id;", new { id });
                // then create umbracoLock object
                db.Execute("INSERT umbracoLock (id, name, value) VALUES (@id, @name, 1);", new { id, name });
                return string.Empty;
            });
        }
    }
}
