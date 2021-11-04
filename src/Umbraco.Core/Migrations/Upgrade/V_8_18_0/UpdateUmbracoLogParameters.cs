
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_18_0
{
    public class UpdateUmbracoLogParameters : MigrationBase
    {
        public UpdateUmbracoLogParameters(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            AlterColumn<LogDto>(Constants.DatabaseSchema.Tables.Log, "logComment");
            AlterColumn<LogDto>(Constants.DatabaseSchema.Tables.Log, "parameters");
        }
    }
}
