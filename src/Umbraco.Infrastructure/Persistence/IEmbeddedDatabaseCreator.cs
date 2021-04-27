namespace Umbraco.Cms.Infrastructure.Persistence
{
    public interface IEmbeddedDatabaseCreator
    {
        string ProviderName { get; }
        string ConnectionString { get; set; }
        void Create();
    }
}
