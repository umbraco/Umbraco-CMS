using Umbraco.Core;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;

namespace Umbraco.Persistance.SqlCe
{
    public class SqlCeEmbeddedDatabaseCreator : IEmbeddedDatabaseCreator
    {
        public string ProviderName => Constants.DatabaseProviders.SqlCe;

        public void Create()
        {
            var engine = new System.Data.SqlServerCe.SqlCeEngine(DatabaseBuilder.EmbeddedDatabaseConnectionString);
            engine.CreateDatabase();
        }
    }
}
