using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Persistence.SqlCe
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
