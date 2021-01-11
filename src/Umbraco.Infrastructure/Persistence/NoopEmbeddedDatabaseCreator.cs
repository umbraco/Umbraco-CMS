namespace Umbraco.Core.Persistence
{
    public class NoopEmbeddedDatabaseCreator : IEmbeddedDatabaseCreator
    {
        public string ProviderName => Constants.DatabaseProviders.SqlServer;

        public string ConnectionString { get; set; }

        public void Create()
        {

        }
    }
}
