namespace Umbraco.Core.Persistence
{
    public interface IEmbeddedDatabaseCreator
    {
        string ProviderName { get; }
        void Create();
    }
}
