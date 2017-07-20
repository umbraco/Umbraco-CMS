using Umbraco.Core.Events;
using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Web.Strategies.Migrations
{
    public interface IPostMigration
    {
        void Migrated(MigrationRunner runner, MigrationEventArgs args);
    }
}
