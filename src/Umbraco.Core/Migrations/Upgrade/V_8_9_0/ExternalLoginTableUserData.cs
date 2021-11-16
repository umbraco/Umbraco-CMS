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
            AddColumn<ExternalLoginDto>(Constants.DatabaseSchema.Tables.ExternalLogin, "userData");
        }
    }
}
