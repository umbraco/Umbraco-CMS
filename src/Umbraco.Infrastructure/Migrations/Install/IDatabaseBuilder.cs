using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.Cms.Infrastructure.Migrations.Install;

public interface IDatabaseBuilder
{
    Task UpgradeSchemaAndData(UmbracoEFCorePlan plan);
}
