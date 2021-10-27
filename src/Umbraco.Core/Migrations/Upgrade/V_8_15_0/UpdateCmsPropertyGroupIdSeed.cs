using NPoco;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_15_0
{
    public class UpdateCmsPropertyGroupIdSeed : MigrationBase
    {
        public UpdateCmsPropertyGroupIdSeed(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            if (DatabaseType.IsSqlCe())
            {
                Database.Execute(Sql("ALTER TABLE [cmsPropertyTypeGroup] ALTER COLUMN [id] IDENTITY (56,1)"));
            }
        }
    }
}
