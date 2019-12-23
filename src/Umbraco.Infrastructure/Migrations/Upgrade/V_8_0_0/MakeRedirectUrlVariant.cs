using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class MakeRedirectUrlVariant : MigrationBase
    {
        public MakeRedirectUrlVariant(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            AddColumn<RedirectUrlDto>("culture");
        }
    }
}
