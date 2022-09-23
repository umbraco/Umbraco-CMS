namespace Umbraco.New.Cms.Infrastructure.Services;

public interface ITemporaryIndexingService
{
    void Set(string indexName);

    void Clear(string? indexName);
}
