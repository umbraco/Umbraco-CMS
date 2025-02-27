using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

[Obsolete("Remove in Umbraco 18.")]
public class AddDocumentUrl : MigrationBase
{
    public AddDocumentUrl(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.DocumentUrl) is false)
        {
            Create.Table<DocumentUrlDto>().Do();
        }
    }
}
