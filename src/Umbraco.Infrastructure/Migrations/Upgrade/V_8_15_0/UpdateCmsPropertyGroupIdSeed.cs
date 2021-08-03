using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_15_0
{
    public class UpdateCmsPropertyGroupIdSeed : MigrationBase
    {
        public UpdateCmsPropertyGroupIdSeed(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (DatabaseType.IsSqlCe())
            {
                Database.Execute(Sql("ALTER TABLE [cmsPropertyTypeGroup] ALTER COLUMN [id] IDENTITY (56,1)"));
            }
        }
    }
}
