namespace Umbraco.Core.Persistence
{
    public interface IEmbeddedDatabaseCreator
    {
        string ProviderName { get; }
        void Create();
    }

    public class NoopEmbeddedDatabaseCreator : IEmbeddedDatabaseCreator
    {
        public string ProviderName => Constants.DatabaseProviders.SqlServer;

        public void Create()
        {

        }
    }
}
