using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddContentTypeIsElementColumn : MigrationBase
    {
        public AddContentTypeIsElementColumn(IMigrationContext context) : base(context)
        { }

        public override void Migrate()
        {
            AddColumn<ContentTypeDto>("isElement");
        }
    }
}
