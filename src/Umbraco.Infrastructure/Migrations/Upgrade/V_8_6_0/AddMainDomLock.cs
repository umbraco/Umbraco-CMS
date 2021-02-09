using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_6_0
{
    public class AddMainDomLock : MigrationBase
    {
        public AddMainDomLock(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.MainDom, Name = "MainDom" });
        }
    }
}
