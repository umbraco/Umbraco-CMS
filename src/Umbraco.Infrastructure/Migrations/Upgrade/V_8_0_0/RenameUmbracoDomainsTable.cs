using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class RenameUmbracoDomainsTable : MigrationBase
{
    public RenameUmbracoDomainsTable(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => Rename.Table("umbracoDomains").To(Constants.DatabaseSchema.Tables.Domain).Do();
}
