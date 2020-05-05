namespace Umbraco.Core.Persistence
{
    public class NoopEmbeddedDatabaseCreator : IEmbeddedDatabaseCreator
    {
        public string ProviderName => Constants.DatabaseProviders.SqlServer;

        public void Create()
        {

        }
    }
}
