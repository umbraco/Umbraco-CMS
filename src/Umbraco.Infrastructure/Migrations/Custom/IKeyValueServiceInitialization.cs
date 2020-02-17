using Umbraco.Core.Persistence;

namespace Umbraco.Infrastructure.Migrations.Custom
{
    public interface IKeyValueServiceInitialization
    {
        void PerformInitialMigration(IUmbracoDatabase database);
    }
}
