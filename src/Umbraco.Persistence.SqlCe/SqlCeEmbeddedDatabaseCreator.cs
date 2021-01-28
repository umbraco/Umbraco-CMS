using Umbraco.Core;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;

namespace Umbraco.Persistence.SqlCe
{
    public class SqlCeEmbeddedDatabaseCreator : IEmbeddedDatabaseCreator
    {
        public string ProviderName => Constants.DatabaseProviders.SqlCe;

        public string ConnectionString { get; set; } = DatabaseBuilder.EmbeddedDatabaseConnectionString;
        public void Create()
        {
            var engine = new System.Data.SqlServerCe.SqlCeEngine(ConnectionString);
            engine.CreateDatabase();
        }
    }
}
