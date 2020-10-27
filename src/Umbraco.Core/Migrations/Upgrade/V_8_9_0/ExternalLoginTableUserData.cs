using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_9_0
{
    public class ExternalLoginTableUserData : MigrationBase
    {
        public ExternalLoginTableUserData(IMigrationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Adds new column to the External Login table
        /// </summary>
        public override void Migrate()
        {
            if (!TableExists(Constants.DatabaseSchema.Tables.ExternalLogin))
            {
                // We may need to create the table if the database was upgraded from a previous release prior to 7.2.x(!)

                Create.Table<ExternalLoginDto>(true).Do();
            } else
            {
                AddColumn<ExternalLoginDto>(Constants.DatabaseSchema.Tables.ExternalLogin, "userData");
            }
        }
    }
}
