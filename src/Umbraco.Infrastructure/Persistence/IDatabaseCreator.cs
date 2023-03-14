namespace Umbraco.Cms.Infrastructure.Persistence;

public interface IDatabaseCreator
{
    string ProviderName { get; }

    void Create(string connectionString);
}
