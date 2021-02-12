using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class MakeTagsVariant : MigrationBase
    {
        public MakeTagsVariant(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            AddColumn<TagDto>("languageId");
        }
    }
}
