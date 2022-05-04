using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class AddContentTypeIsElementColumn : MigrationBase
{
    public AddContentTypeIsElementColumn(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => AddColumn<ContentTypeDto>("isElement");
}
