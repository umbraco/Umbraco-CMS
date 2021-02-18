namespace Umbraco.Core.Persistence
{
    public class NoopEmbeddedDatabaseCreator : IEmbeddedDatabaseCreator
    {
        public string ProviderName => Cms.Core.Constants.DatabaseProviders.SqlServer;

        public string ConnectionString { get; set; }

        public void Create()
        {

        }
    }
}
